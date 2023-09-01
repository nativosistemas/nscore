using System.Net.Sockets;

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
    //Día juliano
    public static double GetJulianDate(DateTime pDateTime)
    {
        double julianDate = pDateTime.ToUniversalTime().ToOADate() + 2415018.5;
        return julianDate;
    }
    // Tiempo Sideral de Greenwich
    public static double GetTSGreenwich(DateTime pDate)
    {
        // Fecha y hora actual en UTC
        // DateTime fechaHora = DateTime.UtcNow;

        // Calcular el número de días transcurridos desde el 1 de enero de 2000 a las 12:00 UT
        TimeSpan ts = pDate - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        double diasTranscurridos = ts.TotalDays;

        // Calcular el tiempo sideral de Greenwich
        double gst = 280.46061837 + 360.98564736629 * diasTranscurridos;
        while (gst < 0.0)
        {
            gst += 360.0;
        }
        while (gst >= 360.0)
        {
            gst -= 360.0;
        }

        return gst;
    }
    // Tiempo Sideral Local
    public static double GetTSL(DateTime pDate, ObserverCoordinates pCity)
    {
        double TSGreenwich = GetTSGreenwich(pDate);

        // Calcular el tiempo sideral local
        double lst = TSGreenwich + pCity.longitude;
        while (lst < 0.0)
        {
            lst += 360.0;
        }
        while (lst >= 360.0)
        {
            lst -= 360.0;
        }

        return lst;
    }
    //
    public static HorariasCoordinates ToHorariasCoordinates(double pSiderealTimeLocal, EquatorialCoordinates pEq)
    {
        double hourAngle_astro = pSiderealTimeLocal - pEq.ra;
        while (hourAngle_astro < 0.0)
        {
            hourAngle_astro += 360.0;
        }
        while (hourAngle_astro >= 360.0)
        {
            hourAngle_astro -= 360.0;
        }
        return new HorariasCoordinates() { dec = pEq.dec, HA = hourAngle_astro };
    }
    public static HorizontalCoordinates ToHorizontalCoordinates(double pSiderealTimeLocal, ObserverCoordinates pCity, EquatorialCoordinates pEq)
    {

        HorariasCoordinates oHorariasCoordinates = ToHorariasCoordinates(pSiderealTimeLocal, pEq);
        double hourAngle_astro = oHorariasCoordinates.HA;

        double altitude = Math.Asin(Math.Sin(pEq.dec * Math.PI / 180.0) * Math.Sin(pCity.latitude * Math.PI / 180.0) + Math.Cos(pEq.dec * Math.PI / 180.0) * Math.Cos(pCity.latitude * Math.PI / 180.0) * Math.Cos(hourAngle_astro * Math.PI / 180.0)) * 180.0 / Math.PI;
        double azimuth = Math.Atan2(Math.Sin(hourAngle_astro * Math.PI / 180.0), Math.Cos(hourAngle_astro * Math.PI / 180.0) * Math.Sin(pCity.latitude * Math.PI / 180.0) - Math.Tan(pEq.dec * Math.PI / 180.0) * Math.Cos(pCity.latitude * Math.PI / 180.0)) * 180.0 / Math.PI;

        // azimut desde el norte
        azimuth -= 180.0;
        // Convertir el azimut a un ángulo entre 0 y 360 grados
        if (azimuth < 0.0)
        {
            azimuth += 360.0;
        }


        return new HorizontalCoordinates() { Altitude = altitude, Azimuth = azimuth };
    }
    public static void ConvertirAFechaJuliana_Main()
    {
          DateTime date = new DateTime(2023, 7, 17, 1, 30, 0).ToUniversalTime();
 double numeroDecimal_fec = date.ToOADate();

        double numeroDecimal = 60142.18785; // Ejemplo de número decimal con el día de fecha juliana

DateTime fecha = DateTime.FromOADate(numeroDecimal);

        DateTime fechaJuliana = ConvertirAFechaJuliana(numeroDecimal);

        Console.WriteLine("Fecha juliana convertida a DateTime: " + fechaJuliana);
    }

    public static DateTime ConvertirAFechaJuliana(double numeroDecimal)
    {
        // Obtener la parte entera y decimal del número decimal
        int anio = (int)numeroDecimal;
        double parteDecimal = numeroDecimal - anio;

        // Obtener el primer día del año en función del año
        DateTime primerDiaDelAnio = new DateTime(anio, 1, 1);

        // Calcular el día del año sumando la parte decimal como días adicionales al primer día del año
        int diasAdicionales = (int)Math.Round(parteDecimal * 365);
        DateTime fechaResultado = primerDiaDelAnio.AddDays(diasAdicionales);

        return fechaResultado;
    }
}