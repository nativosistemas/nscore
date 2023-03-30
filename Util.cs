namespace nscore;
//using System.Collections.Generic;

public class Util
{
    public static string HolaMundo()
    {
        ObserverCoordinates ciudad = ObserverCoordinates.cityQuito;//ObserverCoordinates.cityQuito  cityRosario
        double julianDateGreenwich = AstronomyEngine.GetJulianDate();
        double sirio_Dec = -16.7280; // -16° 42' 58.017''
        double sirio_RA = 101.28326; // 06h 45m 08.9173s
        EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = sirio_Dec, ra = sirio_RA };
        EquatorialCoordinates antares_eq = new EquatorialCoordinates() { dec = -26.4322, ra = 247.35489 };
        HorariasCoordinates antares_horaria = AstronomyEngine.ToHorariasCoordinates(ciudad, antares_eq);//
        HorariasCoordinates sirio_horaria = AstronomyEngine.ToHorariasCoordinates(ciudad, sirio_eq);
        HorizontalCoordinates sirio_horizontal = AstronomyEngine.ToHorizontalCoordinates(ciudad, sirio_eq);
        HorizontalCoordinates antares_horizontal = AstronomyEngine.ToHorizontalCoordinates(ciudad, antares_eq);
        // Mostrar resultantes
        string result = string.Empty;
        result += " - sirio: ";
        result += "Azimuth: " + AstronomyEngine.GetSexagesimal(sirio_horizontal.Azimuth);
        result += " - ";
        result += "Altitud: " + AstronomyEngine.GetSexagesimal(sirio_horizontal.Altitude);

        result += " - antares: ";
        result += "Azimuth: " + AstronomyEngine.GetSexagesimal(antares_horizontal.Azimuth);
        result += " - ";
        result += "Altitud: " + AstronomyEngine.GetSexagesimal(antares_horizontal.Altitude);
        /*
        /*
                     result +=   " - antares: ";
                result += "HA: " + AstronomyEngine.GetHHmmss(antares_horaria.HA);//AstronomyEngine.GetHHmmss
                result += " - ";
                result += "dec: " + AstronomyEngine.GetSexagesimal(antares_horaria.dec);//AstronomyEngine.GetSexagesimal

                     result +=   " - sirio: ";
                result += "HA: " + AstronomyEngine.GetHHmmss(sirio_horaria.HA);//AstronomyEngine.GetHHmmss
                result += " - ";
                result += "dec: " + AstronomyEngine.GetSexagesimal(sirio_horaria.dec);//*/
        //return "Tiempo sideral local: " + AstronomyEngine.GetHHmmss( AstronomyEngine.GetTSL(ciudad_rosario));
        return "Ok: HolaMundo! || sirio -> HorizontalCoordinates: " + result;
    }
}