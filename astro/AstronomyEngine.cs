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
    public static double GetTSGreenwich()
    {
        // Fecha y hora actual en UTC
        DateTime fechaHora = DateTime.UtcNow;

        // Calcular el número de días transcurridos desde el 1 de enero de 2000 a las 12:00 UT
        TimeSpan ts = fechaHora - new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
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
    public static double GetTSL(ObserverCoordinates pCity)
    {
        double TSGreenwich = GetTSGreenwich();

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
    public static HorariasCoordinates ToHorariasCoordinates(ObserverCoordinates pCity, EquatorialCoordinates pEq)
    {
        double siderealTime = GetTSL(pCity);
        // double ra_360 = (pEq.ra * 360.0 / 24.0);
        double hourAngle_astro = siderealTime - pEq.ra;
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
    public static HorizontalCoordinates ToHorizontalCoordinates(ObserverCoordinates pCity, EquatorialCoordinates pEq)
    {

        HorariasCoordinates oHorariasCoordinates = ToHorariasCoordinates(pCity, pEq);
        double hourAngle_astro = oHorariasCoordinates.HA;

        double altitude = Math.Asin(Math.Sin(pEq.dec * Math.PI / 180) * Math.Sin(pCity.latitude * Math.PI / 180) + Math.Cos(pEq.dec * Math.PI / 180) * Math.Cos(pCity.latitude * Math.PI / 180) * Math.Cos(hourAngle_astro * Math.PI / 180)) * 180 / Math.PI;
        double azimuth = Math.Atan2(Math.Sin(hourAngle_astro * Math.PI / 180), Math.Cos(hourAngle_astro * Math.PI / 180) * Math.Sin(pCity.latitude * Math.PI / 180) - Math.Tan(pEq.dec * Math.PI / 180) * Math.Cos(pCity.latitude * Math.PI / 180)) * 180 / Math.PI;

        // Convertir el azimut a un ángulo entre 0 y 360 grados
        if (azimuth < 0.0)
        {
            azimuth += 360.0;
        }


        return new HorizontalCoordinates() { Altitude = altitude, Azimuth = azimuth };
    }
    /*public static HorizontalCoordinates ToHorizontalCoordinates(ObserverCoordinates pCity, EquatorialCoordinates pEq)
    {

        double Azi_decimal = 0;
        double Alt_decimal = 0;
        // Convertir las coordenadas ecuatoriales a coordenadas horizontales
        double siderealTime = GetTSL(pCity);

        // ascensión recta local (ARL)
        // ARL = RA - HSL
        double ARL = siderealTime - pEq.ra;
        // coordenada horaria (HA) 
        // HA = 15 * ARL
        double HA = 15.0 * ARL;
        while (HA < 0.0)
        {
            HA += 360.0;
        }
        while (HA >= 360.0)
        {
            HA -= 360.0;
        }

        //Calcule el ángulo horario (AltH) del objeto como sigue:
        //AltH = sin(Dec) * sin(latitud) + cos(Dec) * cos(latitud) * cos(HA)

        double AltH = Math.Sin(pEq.dec * Math.PI / 180.0) * Math.Sin(pCity.latitude * Math.PI / 180.0) + Math.Cos(pEq.dec * Math.PI / 180.0) * Math.Cos(pCity.latitude * Math.PI / 180.0) * Math.Cos(HA * Math.PI / 180.0);
        //Calcule la altura del objeto (Alt) como sigue:
        //Alt = arcsin(AltH)
        double Alt_radiales = Math.Asin(AltH);

        //Calcule el azimut del objeto (Azi) como sigue:
        //Azi = cos(Dec) * sin(HA) / cos(Alt)
        double Azi_radiales = Math.Cos(pEq.dec * Math.PI / 180.0) * Math.Sin(HA * Math.PI / 180.0) / Math.Cos(Alt_radiales);
        //Si sin(Azi) es positivo, Azi = 360 - Azi.
        double sin_Azi = Math.Sin(Azi_radiales);

        Azi_decimal = (Azi_radiales * (180.0 / Math.PI));

        if (sin_Azi > 0.0)
        {
            Azi_decimal = 360.0 - Azi_decimal;
        }
        Alt_decimal = Alt_radiales * (180.0 / Math.PI);
        //conectarStellarium();
        //////////////


 

        return new HorizontalCoordinates() { Altitude = Alt_decimal, Azimuth = Azi_decimal };
}*/
    public static void conectarStellarium()
    {

        // IP address of the machine running Stellarium
        string ipAddress = "localhost";// "192.168.1.100";
                                       // Port number that Stellarium is listening on
        int port = 10001;

        try
        {
            // Create a new TCP/IP client socket
            TcpClient client = new TcpClient();

            // Connect to Stellarium
            client.Connect(ipAddress, port);

            // Send a message to Stellarium
            string message = "{\"id\":1,\"method\":\"ping\"}\n";
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            // Receive a response from Stellarium
            data = new byte[256];
            string response = string.Empty;
            int bytes = stream.Read(data, 0, data.Length);
            response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine("Received: {0}", response);

            // Close the connection
            client.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
    }

}