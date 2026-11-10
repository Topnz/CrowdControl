/**
 * @file Mobile app logic: sends the participant's position pseudonymously to ingestion-server
 * once the participant has given consent (ID-14), can be stopped at any time (ID-18),
 * and always shows whether the position is being shared right now (ID-22).
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
 * Pseudonymous session ID that follows the positions for as long as sharing is on (ID-16).
 * Generated randomly in the browser on consent and contains nothing about the person or the phone.
 * Only kept in memory; set to null when sharing stops (ID-18), so a new consent gives a new ID.
 * @type {string | null}
 */
let sessionId = null;

/**
 * The event's ID from the URL, or null if the link is missing it.
 * @type {string | null}
 */
const eventId = new URLSearchParams(window.location.search).get("event");

/** The consent button ("Jeg accepterer at dele min position"). @type {HTMLButtonElement} */
const button = document.getElementById("consent");

/** The stop button, shown instead of the consent button while sharing. @type {HTMLButtonElement} */
const stopButton = document.getElementById("stop");

/** The sharing indicator ("Deler position" / "Deler ikke", ID-22). @type {HTMLElement} */
const sharing = document.getElementById("sharing");

/** The line below the indicator that explains why. Read aloud by screen readers (aria-live). @type {HTMLElement} */
const status = document.getElementById("status");

/**
 * ID from setInterval while sharing; otherwise null.
 * @type {number | null}
 */
let timer = null;

showSharing("off");

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
  sessionId = crypto.randomUUID();
  showStopButton(true);
  showSharing("pending");
  status.textContent = "";
  sendPosition();
  timer = setInterval(sendPosition, INTERVAL_MS);
});

// Stop sharing withdraws consent (ID-18)
stopButton.addEventListener("click", () => stopSharing("Du har stoppet delingen."));

/**
 * Stops sharing immediately: no more positions are sent, and the session ID is deleted (ID-18).
 * Requests already under way are ignored when they return (see sendPosition).
 *
 * @param {string} message the reason, shown below the indicator.
 */
function stopSharing(message) {
  clearInterval(timer);
  timer = null;
  sessionId = null;
  showStopButton(false);
  showSharing("off");
  status.textContent = message;
}

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
 * Shows whether the position is being shared right now (ID-22).
 *
 * @param {"on" | "off" | "pending"} state "on" when the server has received the latest position,
 *   "off" when nothing is being counted, and "pending" from consent until the first answer.
 */
function showSharing(state) {
  const texts = { on: "Deler position", off: "Deler ikke", pending: "Starter deling …" };
  sharing.textContent = texts[state];
  sharing.className = "sharing " + state;
}

/**
 * Gets the current position and sends it to ingestion-server with POST /positions.
 * Called once on consent and then every INTERVAL_MS.
 *
 * Answers that arrive after sharing has stopped (or restarted with a new session ID) are ignored,
 * so a late answer cannot show "Deler position" after the participant has pressed stop.
 *
 * If the participant does not grant access to the position, sharing stops and the consent
 * button is shown again, so the participant can try again after granting access.
 */
function sendPosition() {
  const session = sessionId;
  const stillSharing = () => session !== null && session === sessionId;

  navigator.geolocation.getCurrentPosition(
    (pos) => {
      if (!stillSharing()) return;
      fetch("/positions", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          sessionId: session,
          eventId,
          latitude: pos.coords.latitude,
          longitude: pos.coords.longitude,
          // Accuracy in metres, so imprecise positions can be filtered out (ID-40)
          accuracy: pos.coords.accuracy,
          timestamp: new Date(pos.timestamp).toISOString(),
        }),
      })
        .then((response) => {
          if (stillSharing()) showResponse(response);
        })
        .catch(() => {
          if (!stillSharing()) return;
          showSharing("off");
          status.textContent = "Kunne ikke sende din position. Prøver igen.";
        });
    },
    (error) => {
      if (!stillSharing()) return;
      if (error.code === error.PERMISSION_DENIED) {
        // Stop trying, so the user can press again after granting access
        stopSharing("Du har ikke givet adgang til din position.");
      } else {
        // No GPS fix or a timeout; it may work on the next attempt
        showSharing("off");
        status.textContent = "Kunne ikke finde din position. Prøver igen.";
      }
    },
    { enableHighAccuracy: true },
  );
}

/**
 * Shows the server's response in the indicator and the status line.
 * fetch only fails on network problems, so the server's status code must be checked here.
 * On errors the app keeps trying, so sharing resumes by itself, e.g. when the participant
 * walks into the event's area.
 *
 * - 2xx: the position was received.
 * - 422: the position is outside the event's area, or the event is not active (ID-41).
 * - 429: the app has sent too often (rate limiting, ID-28); nothing is changed.
 *
 * @param {Response} response the response from POST /positions.
 */
function showResponse(response) {
  if (response.ok) {
    showSharing("on");
    status.textContent = "";
  } else if (response.status === 422) {
    showSharing("off");
    status.textContent = "Du er uden for eventets område, eller eventet er ikke aktivt.";
  } else if (response.status === 429) {
    // The app sent slightly too often; it is not the user's fault, so nothing is changed
  } else {
    showSharing("off");
    status.textContent = "Der skete en fejl. Prøv at genindlæse siden.";
  }
}
