/**
 * @file Mobile app logic: sends the participant's position pseudonymously to ingestion-server
 * once the participant has given consent (ID-14).
 *
 * The payload must follow contracts/openapi/ingestion-api.yaml (POST /positions).
 * The page is opened with the event's ID in the URL: /?event=<eventId>
 */

/**
 * How often the position is sent, in milliseconds.
 * TODO (ID-29): Must be fetched from the event, so the organiser decides the interval.
 * @type {number}
 */
const INTERVAL_MS = 5000;

/**
 * Pseudonymous session ID that follows the positions for as long as sharing is on.
 * Generated randomly in the browser and contains nothing about the person or the phone (ID-16).
 * @type {string}
 */
const sessionId = crypto.randomUUID();

/**
 * The event's ID from the URL, or null if the link is missing it.
 * @type {string | null}
 */
const eventId = new URLSearchParams(window.location.search).get("event");

/** The consent button ("Jeg accepterer at dele min position"). @type {HTMLButtonElement} */
const button = document.getElementById("consent");

/** The stop button, shown instead of the consent button while sharing. @type {HTMLButtonElement} */
const stopButton = document.getElementById("stop");

/** The status line below the buttons. Read aloud by screen readers (aria-live). @type {HTMLElement} */
const status = document.getElementById("status");

/**
 * ID from setInterval while sharing; otherwise null.
 * @type {number | null}
 */
let timer = null;

// Without an eventId the server cannot link the position to an event, so nothing is sent
if (!eventId) {
  button.disabled = true;
  status.textContent = "Linket mangler et event. Brug det link, du har fået af arrangøren.";
}

// The Geolocation API is only called here, after consent (ID-14)
button.addEventListener("click", () => {
  if (!("geolocation" in navigator)) {
    status.textContent = "Din browser understøtter ikke positionsdeling.";
    return;
  }
  showStopButton(true);
  sendPosition();
  timer = setInterval(sendPosition, INTERVAL_MS);
});

// TODO (ID-18): Stop sharing must stop the timer and delete the session ID. The button does nothing yet.

/**
 * Switches between the consent button and the stop button.
 * While sharing, the consent button is replaced by the stop button.
 *
 * @param {boolean} sharing true while sharing, so the stop button is shown; false shows the consent button again.
 */
function showStopButton(sharing) {
  button.hidden = sharing;
  stopButton.hidden = !sharing;
}

/**
 * Gets the current position and sends it to ingestion-server with POST /positions.
 * Called once on consent and then every INTERVAL_MS.
 *
 * If the participant does not grant access to the position, sharing stops and the consent
 * button is shown again, so the participant can try again after granting access.
 */
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
          // Accuracy in metres, so imprecise positions can be filtered out (ID-40)
          accuracy: pos.coords.accuracy,
          timestamp: new Date(pos.timestamp).toISOString(),
        }),
      })
        .then(showResponse)
        .catch(() => (status.textContent = "Kunne ikke sende position."));
    },
    () => {
      // Stop trying, so the user can press again after granting access
      clearInterval(timer);
      status.textContent = "Du har ikke givet adgang til din position.";
      showStopButton(false);
    },
    { enableHighAccuracy: true },
  );
}

/**
 * Shows the server's response in the status line.
 * fetch only fails on network problems, so the server's status code must be checked here.
 *
 * - 2xx: the position was received.
 * - 422: the position is outside the event's area, or the event is not active (ID-41).
 * - 429: the app has sent too often (rate limiting, ID-28); the message is not changed.
 *
 * @param {Response} response the response from POST /positions.
 */
function showResponse(response) {
  if (response.ok) {
    status.textContent = "Din position deles.";
  } else if (response.status === 422) {
    status.textContent = "Du er uden for eventets område, eller eventet er ikke aktivt.";
  } else if (response.status === 429) {
    // The app sent slightly too often; it is not the user's fault, so the message is not changed
  } else {
    status.textContent = "Der skete en fejl. Prøv at genindlæse siden.";
  }
}
