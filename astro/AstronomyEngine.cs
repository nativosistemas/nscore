namespace nscore;
public class AstronomyEngine
{
    public static string GetHHmmss(double pValur)
    {
        TimeSpan tiempoSideral = TimeSpan.FromHours(pValur / 15.0);
        return tiempoSideral.ToString(@"hh\:mm\:ss");
    }
    public static string GetSexagesimal(double angulo)
    {
        int grados = (int)Math.Floor(angulo);
        double minutosDecimal = (angulo - grados) * 60;
        int minutos = (int)Math.Floor(minutosDecimal);
        double segundosDecimal = (minutosDecimal - minutos) * 60;

        return $"{grados}° {minutos}' {Math.Round(segundosDecimal, 4)}\"";
    }
    public static double GetJulianDate()
    {
        double julianDate = DateTime.UtcNow.ToOADate() + 2415018.5;
        return julianDate;
    }
    // Tiempo Sideral de Greenwich
    public static double GetTSGreenwich()
    {
        // Fecha y hora actual en UTC
        DateTime fechaHora = DateTime.UtcNow;

        // Calcular el número de días transcurridos desde el 1 de enero de 2000 a las 12:00 UT
        TimeSpan ts = fechaHora - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        double diasTranscurridos = ts.TotalDays;

        // Calcular el tiempo sideral de Greenwich
        double gst = 280.46061837 + 360.98564736629 * diasTranscurridos;
        while (gst < 0)
        {
            gst += 360;
        }
        while (gst >= 360)
        {
            gst -= 360;
        }

        return gst;
    }
    // Tiempo Sideral Local
    public static double GetTSL(ObserverCoordinates pCity)
    {
        double TSGreenwich = GetTSGreenwich();

        // Calcular el tiempo sideral local
        double lst = TSGreenwich + pCity.longitude;
        while (lst < 0)
        {
            lst += 360;
        }
        while (lst >= 360)
        {
            lst -= 360;
        }

        return lst;
    }
    //Día juliano
    public static double ToJulianDate(DateTime pDateTime)
    {
        double julianDate = pDateTime.ToOADate() + 2415018.5;
        return julianDate;
    }
    //
    public static HorizontalCoordinates ToHorizontalCoordinates(ObserverCoordinates pCity, EquatorialCoordinates pEq)
    {
        // Convertir las coordenadas ecuatoriales a coordenadas horizontales
        double siderealTime = GetTSL(pCity);
        double hourAngle = siderealTime - pEq.ra;
        double altitude = Math.Asin(Math.Sin(pEq.dec * Math.PI / 180.0) * Math.Sin(pCity.latitude * Math.PI / 180.0) + Math.Cos(pEq.dec * Math.PI / 180.0) * Math.Cos(pCity.latitude * Math.PI / 180.0) * Math.Cos(hourAngle * Math.PI / 180.0)) * 180.0 / Math.PI;
        double azimuth = Math.Atan2(Math.Sin(hourAngle * Math.PI / 180.0), Math.Cos(hourAngle * Math.PI / 180.0) * Math.Sin(pCity.latitude * Math.PI / 180.0) - Math.Tan(pEq.dec * Math.PI / 180.0) * Math.Cos(pCity.latitude * Math.PI / 180.0)) * 180.0 / Math.PI;

        // Convertir el azimut a un ángulo entre 0 y 360 grados
        if (azimuth < 0)
        {
            azimuth += 360;
        }


        return new HorizontalCoordinates() { Altitude = altitude, Azimuth = azimuth };
    }


}