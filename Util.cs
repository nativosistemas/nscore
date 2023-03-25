namespace nscore;
//using System.Collections.Generic;

public class Util
{
    public static string HolaMundo()
    {
        ObserverCoordinates ciudad_rosario = ObserverCoordinates.cityRosario;
        double julianDateGreenwich = AstronomyEngine.ToJulianDate(DateTime.Now);
        double sirio_Dec = -16.71611583; // -16° 42' 58.017''
        double sirio_RA = 6.752477028; // 06h 45m 08.9173s
        EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = sirio_Dec, ra = sirio_RA };
        HorizontalCoordinates hor = AstronomyEngine.ToHorizontalCoordinates(ciudad_rosario, sirio_eq);
        // Mostrar resultantes
        string result = string.Empty;
        result += "Azimuth: " + AstronomyEngine.GetSexagesimal(hor.Azimuth);
        result += " - ";
        result += "Altitud: " + AstronomyEngine.GetSexagesimal(hor.Altitude);


 //return "Tiempo sideral local: " + AstronomyEngine.GetHHmmss( AstronomyEngine.GetTSL(ciudad_rosario));
        return "Ok: HolaMundo! || sirio -> HorizontalCoordinates: " + result;
    }
}