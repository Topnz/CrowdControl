// Sender brugerens position anonymt til ingestion-server, når brugeren har givet samtykke.
// Payload skal følge contracts/openapi/ingestion-api.yaml (POST /positions).

const INTERVAL_MS = 10000;
const sessionId = crypto.randomUUID();

const button = document.getElementById("consent");
const status = document.getElementById("status");

button.addEventListener("click", () => {
  if (!("geolocation" in navigator)) {
    status.textContent = "Din browser understøtter ikke positionsdeling.";
    return;
  }
  button.disabled = true;
  sendPosition();
  setInterval(sendPosition, INTERVAL_MS);
});

function sendPosition() {
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      fetch("/positions", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          sessionId,
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
          accuracy: pos.coords.accuracy,
          timestamp: new Date(pos.timestamp).toISOString(),
        }),
      })
        .then(() => (status.textContent = "Din position deles."))
        .catch(() => (status.textContent = "Kunne ikke sende position."));
    },
    () => {
      status.textContent = "Du har ikke givet adgang til din position.";
      button.disabled = false;
    },
    { enableHighAccuracy: true },
  );
}