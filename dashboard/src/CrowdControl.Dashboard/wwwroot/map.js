let map;
let mapDotNetReference
let heatLayer;

window.initMap = function (dotnetReference) {
    mapDotNetReference = dotnetReference

    // A map left over from the previous page would otherwise keep its layers and listeners
    window.disposeMap();

    map = L.map('map').setView([55.6761, 12.5683], 8);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    heatLayer = L.layerGroup().addTo(map);
};

window.disposeMap = function () {
    if (map) {
        map.remove();
        map = null;
        heatLayer = null;
    }
};

// Draws a HeatmapSnapshotDto: one dot per grid cell in the middle of the cell, coloured by count
window.renderHeatmap = function (snapshot) {
    if (!map || !heatLayer) return;
    heatLayer.clearLayers();

    const maxCount = Math.max(1, ...snapshot.cells.map(cell => cell.count));
    for (const cell of snapshot.cells) {
        const lat = snapshot.minLat + (cell.y + 0.5) * snapshot.cellSizeLat;
        const lon = snapshot.minLon + (cell.x + 0.5) * snapshot.cellSizeLon;
        const intensity = cell.count / maxCount;
        // green (low) → red (high)
        const color = `hsl(${Math.round(120 * (1 - intensity))}, 90%, 45%)`;
        L.circleMarker([lat, lon], {
            radius: 4 + 6 * intensity,
            color: color,
            fillColor: color,
            fillOpacity: 0.7,
            weight: 1
        }).bindTooltip(`${cell.count}`).addTo(heatLayer);
    }
};

// Draws a polygon on the map
window.polygonDrawing = (function () {

    let map = null;
    let dotNetReference = null;

    let points = [];
    let markers = [];

    let polygon = null;
    let previewLine = null;

    let drawingLayer = null;
    let isDrawing = false;

    function startDrawing(mapInstance, dotNetRef) {
        map = mapInstance;
        dotNetReference = dotNetRef;

        clearDrawing();

        drawingLayer = L.featureGroup().addTo(map);

        points = [];
        markers = [];
        isDrawing = true;

        map.on('click', onMapClick);
        map.on('mousemove', onMouseMove);
    }

    function onMapClick(e) {
        if (!isDrawing) return;

        const point = e.latlng;

        points.push({
            latitude: point.lat,
            longitude: point.lng
        });

        // Add vertex marker
        const marker = L.circleMarker(point, {
            radius: 5,
            color: '#2563eb',
            fillColor: '#ffffff',
            fillOpacity: 1
        });

        marker.addTo(drawingLayer);
        markers.push(marker);

        updatePolygon();
    }

    function onMouseMove(e) {
        if (!isDrawing || points.length === 0) return;

        const coordinates = points.map(p => [
            p.latitude,
            p.longitude
        ]);

        coordinates.push([
            e.latlng.lat,
            e.latlng.lng
        ]);

        if (previewLine) {
            drawingLayer.removeLayer(previewLine);
        }
    }

    function updatePolygon() {
        if (previewLine) {
            drawingLayer.removeLayer(previewLine);
            previewLine = null;
        }

        if (polygon) {
            drawingLayer.removeLayer(polygon);
            polygon = null;
        }

        if (points.length < 2) return;

        const coordinates = points.map(p => [
            p.latitude,
            p.longitude
        ]);

        polygon = L.polygon(coordinates, {
            color: '#2563eb',
            weight: 2,
            fillColor: '#3b82f6',
            fillOpacity: 0.2
        }).addTo(drawingLayer);
    }

    async function finishDrawing() {
        if (!isDrawing || points.length < 3) {
            return;
        }
        
        isDrawing = false;

        map.off('click', onMapClick);
        map.off('mousemove', onMouseMove);

        // Close polygon
        const coordinates = [...points];

        const first = coordinates[0];
        const last = coordinates[coordinates.length - 1];

        if (
            first.latitude !== last.latitude ||
            first.longitude !== last.longitude
        ) {
            coordinates.push({ ...first });
        }

        // Send coordinates to C#
        await dotNetReference.invokeMethodAsync(
            'OnPolygonCompleted',
            coordinates
        );

        points = [];
        markers = [];
        polygon = null;
        previewLine = null;
    }

    function cancelDrawing() {
        if (map) {
            map.off('click', onMapClick);
            map.off('mousemove', onMouseMove);
        }

        isDrawing = false;
        points = [];
        markers = [];
        polygon = null;
        previewLine = null;

        if (drawingLayer) {
            drawingLayer.clearLayers();
        }
    }

    function clearDrawing() {
        if (drawingLayer) {
            drawingLayer.clearLayers();
        }
    }

    return {
        startDrawing,
        finishDrawing,
        cancelDrawing
    };

})();

window.startPolygonDrawing = function () {
    window.polygonDrawing.startDrawing(
        map,
        mapDotNetReference
    );
};

window.finishPolygonDrawing = function () {
    return window.polygonDrawing.finishDrawing();
};

window.cancelPolygonDrawing = function () {
    window.polygonDrawing.cancelDrawing();
};