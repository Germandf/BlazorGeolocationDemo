using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

namespace BlazorGeolocationDemo.Services;

public class KmlService : IKmlService
{
    private readonly double _tsAsLatitude = -38.377048;
    private readonly double _tsAsLongitude = -60.275883;
    private readonly int _tsAsMeters = 7000;

    public void GenerateCity()
    {
        GetBoundingBox(_tsAsLatitude, _tsAsLongitude, _tsAsMeters, 
            out double deltaLat, out double deltaLon);

        var vector1 = new Vector() { Latitude = _tsAsLatitude - deltaLat, Longitude = _tsAsLongitude + deltaLon };
        var vector2 = new Vector() { Latitude = _tsAsLatitude - deltaLat, Longitude = _tsAsLongitude - deltaLon };
        var vector3 = new Vector() { Latitude = _tsAsLatitude + deltaLat, Longitude = _tsAsLongitude - deltaLon };
        var vector4 = new Vector() { Latitude = _tsAsLatitude + deltaLat, Longitude = _tsAsLongitude + deltaLon };
        var vector5 = new Vector() { Latitude = _tsAsLatitude - deltaLat, Longitude = _tsAsLongitude + deltaLon };

        var polygon = new Polygon()
        {
            OuterBoundary = new() { LinearRing = new() { Coordinates = new() { vector1, vector2, vector3, vector4, vector5 } } }
        };

        var lineString = new LineString
        {
            Coordinates = new() { vector1, vector2, vector3, vector4, vector5 }
        };

        var placemark = new Placemark
        {
            // polygon could be replaced by lineString
            Geometry = polygon,
            Name = "Tres Arroyos",
        };

        KmlFile kml = KmlFile.Create(placemark, false);

        using (FileStream stream = File.Create("wwwroot/kml/tres-arroyos.kml"))
        {
            kml.Save(stream);
        }
    }

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

    private void GetBoundingBox(
        double pLatitude, double pLongitude, int pDistanceInMeters, 
        out double deltaLat, out double deltaLong)
    {
        double latRadian = (Math.PI / 180) * pLatitude;
        double degLatKm = 110.574235;
        double degLongKm = 110.572833 * Math.Cos(latRadian);

        deltaLat = pDistanceInMeters / 1000.0 / degLatKm;
        deltaLong = pDistanceInMeters / 1000.0 / degLongKm;
    }

    public record City(string Name, List<Vector> Polygons);

}
