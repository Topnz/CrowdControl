package dk.crowdcontrol.ingestion.domain;

import java.util.Objects;
import java.util.UUID;

/**
 * Pseudonymous ID for one device. Generated randomly in the browser on consent and
 * deleted again on stop (ID-18), so it cannot be traced back to a person.
 * Used for rate limiting and to see that positions come from the same device.
 */
public class SessionId
{
  private final UUID value;

  /**
   * @param value the ID as a UUID
   * @throws NullPointerException if value is null
   */
  public SessionId(UUID value)
  {
    this.value = Objects.requireNonNull(value, "value cannot be null");
  }

  /**
   * @param value the ID as text, e.g. "3f2c1a9e-6b7d-4e8f-9a0b-1c2d3e4f5a6b"
   * @return the session ID
   * @throws IllegalArgumentException if the text is not a valid UUID
   */
  public static SessionId of(String value)
  {
    return new SessionId(UUID.fromString(value));
  }

  /** @return the ID as a UUID */
  public UUID getValue()
  {
    return value;
  }

  /** Two session IDs are equal if they have the same UUID. */
  @Override
  public boolean equals(Object o)
  {
    return o instanceof SessionId other && value.equals(other.value);
  }

  @Override
  public int hashCode()
  {
    return value.hashCode();
  }

  /** @return the UUID as text, so it can be written directly to the log */
  @Override
  public String toString()
  {
    return value.toString();
  }
}
