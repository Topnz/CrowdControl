// Sender brugerens position anonymt til ingestion-server, når brugeren har givet samtykke.
// Payload skal følge contracts/openapi/ingestion-api.yaml (POST /positions).
// Siden åbnes med eventets ID i URL'en: /?event=<eventId>

const INTERVAL_MS = 5000;
const sessionId = crypto.randomUUID();
const eventId = new URLSearchParams(window.location.search).get("event");

const button = document.getElementById("consent");
const status = document.getElementById("status");

let timer = null;

// Uden eventId kan serveren ikke knytte positionen til et event, så der sendes intet
if (!eventId) {
  button.disabled = true;
  status.textContent = "Linket mangler et event. Brug det link, du har fået af arrangøren.";
}

button.addEventListener("click", () => {
  if (!("geolocation" in navigator)) {
    status.textContent = "Din browser understøtter ikke positionsdeling.";
    return;
  }
  button.disabled = true;
  sendPosition();
  timer = setInterval(sendPosition, INTERVAL_MS);
});

function sendPosition() {
  navigator.geolocation.getCurrentPosition(
    (pos) => {
      fetch("/positions", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          sessionId,
          eventId,
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
          accuracy: pos.coords.accuracy,
          timestamp: new Date(pos.timestamp).toISOString(),
        }),
      })
        .then(showResponse)
        .catch(() => (status.textContent = "Kunne ikke sende position."));
    },
    () => {
      // Stop med at prøve, så brugeren kan trykke igen efter at have givet adgang
      clearInterval(timer);
      status.textContent = "Du har ikke givet adgang til din position.";
      button.disabled = false;
    },
    { enableHighAccuracy: true },
  );
}

// fetch fejler kun ved netværksproblemer, så serverens statuskode skal tjekkes her
function showResponse(response) {
  if (response.ok) {
    status.textContent = "Din position deles.";
  } else if (response.status === 422) {
    status.textContent = "Du er uden for eventets område, eller eventet er ikke aktivt.";
  } else if (response.status === 429) {
    // Appen har sendt lidt for tit; det er ikke brugerens fejl, så beskeden ændres ikke
  } else {
    status.textContent = "Der skete en fejl. Prøv at genindlæse siden.";
  }
}
