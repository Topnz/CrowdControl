package dk.crowdcontrol.ingestion.domain;

import java.time.LocalDateTime;
import java.util.ArrayList;

public class Event
{
  private String name;
  private LocalDateTime startTime;
  private LocalDateTime endTime;
  private int sendInterval;
  private Boundary perimiter;
  private ArrayList<Zone> zones;

  public Event()
  {

  }
}
