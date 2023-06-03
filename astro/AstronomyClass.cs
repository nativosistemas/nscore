namespace nscore;

public class ObserverCoordinates
{
    // Coordenadas geográficas del observador
    public double latitude { get; set; }//= 19.4326; // en grados
    public double longitude { get; set; }//= -99.1332; // en grados
    public double altitude { get; set; }//= 0.0; // en metros

    public static ObserverCoordinates cityRosario = new ObserverCoordinates() { latitude = -32.9575, longitude = -60.639444, altitude = 0 };
    public static ObserverCoordinates cityQuito = new ObserverCoordinates() { latitude = -0.22, longitude = -78.5125, altitude = 0 };
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

    // public double ra_radianes { get { return ra * Math.PI / 180.0; } }
    //public double dec_radianes { get { return dec * Math.PI / 180.0; } }
    //public EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = sirio_Dec, ra = sirio_RA };

}
public class HorariasCoordinates
{
    public double dec = 0; // en grados
    public double HA = 0; // en grados
}
public class ServoCoordinates
{
    private double _servoH { get; set; }
    private double _servoV { get; set; }
    public double servoH { get { return _servoH; } set { if (value >= 0.0 && value <= 180.0) { _servoH = value; } } }
    public double servoV { get { return _servoV; } set { if (value >= 0.0 && value <= 180.0) { _servoV = value; } } }


    public static ServoCoordinates convertServoCoordinates(HorizontalCoordinates pValue)
    {
        bool isAzimuthMas180 = false;
        double horizontal = pValue.Azimuth;
        double vertical = pValue.Altitude;
        if (pValue.Azimuth > 180.0)
        {
            isAzimuthMas180 = true;
            horizontal = 180.0 - (pValue.Azimuth - 180.0);
        }
        if (isAzimuthMas180) { 
            vertical = 180.0 - pValue.Altitude ;
        }
        return new ServoCoordinates() { servoH = horizontal, servoV = vertical };
    }
}