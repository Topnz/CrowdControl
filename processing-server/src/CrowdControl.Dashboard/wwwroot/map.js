let map;
let marker;
let heatLayer;

window.initMap = function () {
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

window.addMapPoint = function (latitude, longitude) {
    L.marker([latitude, longitude])
        .addTo(map)
        .bindPopup();
};

// Draws a HeatmapSnapshotDto: one dot per grid cell in the middle of the cell, coloured by count
window.renderHeatmap = function (snapshot) {
    if (!map || !heatLayer) return;
    heatLayer.clearLayers();

    const max = Math.max(1, ...snapshot.cells.map(c => c.count));
    for (const cell of snapshot.cells) {
        const lat = snapshot.minLat + (cell.y + 0.5) * snapshot.cellSizeLat;
        const lon = snapshot.minLon + (cell.x + 0.5) * snapshot.cellSizeLon;
        const t = cell.count / max;
        // green (low) → red (high)
        const color = `hsl(${Math.round(120 * (1 - t))}, 90%, 45%)`;
        L.circleMarker([lat, lon], {
            radius: 4 + 6 * t,
            color: color,
            fillColor: color,
            fillOpacity: 0.7,
            weight: 1
        }).bindTooltip(`${cell.count}`).addTo(heatLayer);
    }
};
