namespace CrowdControl.Application;

public static class PolygonHelper
{
    public static bool IsInside(IReadOnlyList<GeoPoint> corners, GeoPoint point)
    {
        bool inside = false;
        int previous = corners.Count - 1;

        for (int i = 0; i < corners.Count; i++)
        {
            GeoPoint a = corners[i];
            GeoPoint b = corners[previous];

            bool edgeCrossesPointLatitude =
                (a.Latitude > point.Latitude) != (b.Latitude > point.Latitude);

            if (edgeCrossesPointLatitude)
            {
                double crossingLongitude = a.Longitude +
                                           (point.Latitude - a.Latitude) * (b.Longitude - a.Longitude)
                                           / (b.Latitude - a.Latitude);

                if (point.Longitude < crossingLongitude)
                {
                    inside = !inside;
                }
            }

            previous = i;
        }

        return inside;
    }
}