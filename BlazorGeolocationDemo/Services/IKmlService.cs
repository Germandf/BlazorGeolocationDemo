namespace BlazorGeolocationDemo.Services;

public interface IKmlService
{
    string? GetCity(double latitude, double longitude);
}
