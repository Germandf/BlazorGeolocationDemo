using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace BlazorGeolocationDemo.Services;

public class KmlService : IKmlService
{
    public string? GetCity(double latitude, double longitude)
    {
        List<City> cities = new();

        foreach (var filePath in Directory.GetFiles("wwwroot/kml"))
        {
            var stream = File.OpenRead(filePath);
            var file = KmlFile.Load(stream);
            var placemark = file.Root.Flatten().OfType<Placemark>().FirstOrDefault();

            var city = new City(placemark?.Name ?? "", new());

            foreach (var poly in file.Root.Flatten().OfType<Polygon>())
                foreach (var coordinates in file.Root.Flatten().OfType<CoordinateCollection>())
                    foreach (var coordinate in coordinates)
                        city.Polygons.Add(coordinate);

            foreach (var lineString in file.Root.Flatten().OfType<LineString>())
                foreach (var coordinates in file.Root.Flatten().OfType<CoordinateCollection>())
                    foreach (var coordinate in coordinates)
                        city.Polygons.Add(coordinate);

            cities.Add(city);
        }

        foreach (var city in cities)
        {
            if (IsPointInside(city.Polygons.ToArray(), new() { Latitude = latitude, Longitude = longitude }))
                return city.Name;
        }
        
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

    public record City(string Name, List<Vector> Polygons);

}
