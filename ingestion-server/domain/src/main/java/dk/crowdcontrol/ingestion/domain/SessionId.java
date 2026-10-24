package dk.crowdcontrol.ingestion.domain;

import java.util.Objects;
import java.util.UUID;

/**
 * Anonymt ID for én enhed. Genereres tilfældigt i browseren ved samtykke og
 * slettes igen ved stop, så det kan ikke føres tilbage til en person.
 */
public class SessionId
{
  private final UUID value;

  public SessionId(UUID value)
  {
    this.value = Objects.requireNonNull(value, "value cannot be null");
  }

  public static SessionId of(String value)
  {
    return new SessionId(UUID.fromString(value));
  }

  public UUID getValue()
  {
    return value;
  }

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

  @Override
  public String toString()
  {
    return value.toString();
  }
}
