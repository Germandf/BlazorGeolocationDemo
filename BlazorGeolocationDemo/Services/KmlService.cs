using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace BlazorGeolocationDemo.Services;

public class KmlService : IKmlService
{
    public string? GetCity(double latitude, double longitude)
    {
        var stream = File.OpenRead("wwwroot/kml/tres-arroyos.kml");
        var file = KmlFile.Load(stream);
        var placemark = file.Root.Flatten().OfType<Placemark>().FirstOrDefault();
        List<Vector> polygon = new();

        foreach (var poly in file.Root.Flatten().OfType<Polygon>())
            foreach (var coordinates in file.Root.Flatten().OfType<CoordinateCollection>())
                foreach (var coordinate in coordinates)
                    polygon.Add(coordinate);

        if (IsPointInside(polygon.ToArray(), new() { Latitude = latitude, Longitude = longitude }))
            return placemark?.Name;
        
        return null;
    }

    private bool IsPointInside(Vector[] polygon, Vector testPoint)
    {
        bool result = false;
        int j = polygon.Count() - 1;

        for (int i = 0; i < polygon.Count(); i++)
        {
            if (polygon[i].Latitude < testPoint.Latitude && polygon[j].Latitude >= testPoint.Latitude || polygon[j].Latitude < testPoint.Latitude && polygon[i].Latitude >= testPoint.Latitude)
                if (polygon[i].Longitude + (testPoint.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) * (polygon[j].Longitude - polygon[i].Longitude) < testPoint.Longitude)
                    result = !result;
            j = i;
        }

        return result;
    }

}
