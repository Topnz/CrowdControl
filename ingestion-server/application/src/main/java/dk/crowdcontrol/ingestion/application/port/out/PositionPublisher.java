package dk.crowdcontrol.ingestion.application.port.out;

import dk.crowdcontrol.ingestion.domain.FilteredPosition;

public interface PositionPublisher
{
  void publish(FilteredPosition filteredPosition);
}
