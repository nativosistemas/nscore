namespace nscore;

public class ObserverCoordinates
{
    // Coordenadas geográficas del observador
    public double latitude { get; set; }//= 19.4326; // en grados
    public double longitude { get; set; }//= -99.1332; // en grados
    public double altitude { get; set; }//= 0.0; // en metros
    public TimeZoneInfo timeZoneInfo { get; set; }
    public double timeZone
    {
        get
        {
            return timeZoneInfo.BaseUtcOffset.Hours;
        }
    }
    public static ObserverCoordinates cityRosario = new ObserverCoordinates() { latitude = -32.9575, longitude = -60.639444, altitude = 0, timeZoneInfo = TimeZoneInfo.Local };
    public static ObserverCoordinates cityQuito = new ObserverCoordinates() { latitude = -0.22, longitude = -78.5125, altitude = 0, timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time") };
}
public class HorizontalCoordinates
{
    public double Altitude { get; set; }
    public double Azimuth { get; set; }
}
public class EquatorialCoordinates
{
    public double ra = 0; // en horas
    public double dec = 0; // en grados
    public double epoch = 2000.0; // en años julianos

    //public EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = sirio_Dec, ra = sirio_RA };

}