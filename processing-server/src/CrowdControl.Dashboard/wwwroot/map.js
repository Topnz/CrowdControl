let map;
let marker;

window.initMap = function () {
    map = L.map('map').setView([55.6761, 12.5683], 8);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);
};

window.disposeMap = function () {
    if (map) {
        map.remove();
        map = null;
    }
};

window.addMapPoint = function (latitude, longitude) {
    L.marker([latitude, longitude])
        .addTo(map)
        .bindPopup();
};