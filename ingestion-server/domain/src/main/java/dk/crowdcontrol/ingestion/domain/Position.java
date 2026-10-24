package dk.crowdcontrol.ingestion.domain;

import java.time.LocalDateTime;

public class Position
{
  private LocalDateTime timestamp;
  private int accuracy;
  private GeoPoint geoPoint;

  public Position()
  {

  }
}
