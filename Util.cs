namespace nscore;
//using System.Collections.Generic;

public class Util
{
    public static string WebRootPath { get; set; }

    public static string HolaMundo()
    {
        //
        //  ServoController device = new ServoController();
        //  device.logica();
        DKbase.generales.Log.LogErrorFile("HolaMundo", "ex.ToString()");
        // 


        ObserverCoordinates ciudad = ObserverCoordinates.cityRosario;//ObserverCoordinates.cityQuito  cityRosario
        //double julianDateGreenwich = AstronomyEngine.GetJulianDate();
        double siderealTime_local = AstronomyEngine.GetTSL(ciudad);
        double sirio_Dec = -16.7280; // -16° 42' 58.017''
        double sirio_RA = 101.28326; // 06h 45m 08.9173s
        EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = sirio_Dec, ra = sirio_RA };
        EquatorialCoordinates antares_eq = new EquatorialCoordinates() { dec = -26.4832, ra = 247.70873 };
        HorariasCoordinates antares_horaria = AstronomyEngine.ToHorariasCoordinates(siderealTime_local, antares_eq);//
        HorariasCoordinates sirio_horaria = AstronomyEngine.ToHorariasCoordinates(siderealTime_local, sirio_eq);
        HorizontalCoordinates sirio_horizontal = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, ciudad, sirio_eq);
        HorizontalCoordinates antares_horizontal = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, ciudad, antares_eq);
        // Mostrar resultantes
        string result = string.Empty;
        result += " - sirio: ";
        result += "Azimuth: " + (sirio_horizontal.Azimuth);//AstronomyEngine.GetSexagesimal
        result += " - ";
        result += "Altitud: " + (sirio_horizontal.Altitude);

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

        //string directorioActual = System.IO.Directory.GetCurrentDirectory();
        //string rutaApp = System.Reflection.Assembly.GetEntryAssembly().Location;

        return "Ok: HolaMundo! || sirio -> HorizontalCoordinates: " + result;
    }
    public static List<Star> getStars()
    {
        List<Star> l = new List<Star>();
        try
        {
            //  builder.Environment.WebRootPath
            // var nameFile = Path.Combine(Directory.GetCurrentDirectory(), @"files", "Las Estrellas mas Brilantes.txt");

            var nameFile = Path.Combine(nscore.Util.WebRootPath, @"files", "Las Estrellas mas Brilantes.txt");
            if (File.Exists(nameFile))
            {
                using (var sr = new StreamReader(nameFile))
                {
                    int cont = 0;
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        cont++;
                        Star o = new Star();
                       // o.id = cont;
                        o.nameBayer = Convert.ToInt32(line.Substring(0, 4).Replace(@",", string.Empty));
                        o.name = line.Substring(30, 18).Trim();
                        string[] ra = line.Substring(48, 6).Trim().Split(new Char[] { ' ' });
                        o.ra = ((Convert.ToDouble(ra[0]) + (Convert.ToDouble(ra[1]) / 60)) * 360.0) / 24.0;
                        o.dec = Convert.ToDouble(line.Substring(54, 8));
                        l.Add(o);
                        //Console.WriteLine(line);
                    }
                }
            }
        }
        catch (IOException e)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), e, DateTime.Now);
            Console.WriteLine(e.Message);
        }
        return l;
    }
    public static void initDbContext()
    {
        string strDataSource = Path.Combine(nscore.Helper.folder, nscore.Helper.sqllite);
        using (var context = new AstroDbContext())
        {
            context.Database.EnsureCreated(); // Crea la base de datos si no existe
            var ddd = context.Stars.ToList();
            var canti = ddd.Count;
            var yyy = 5 + 7;
             var l = getStars();
              foreach (var item in l)
              {
                  context.Stars.Add(item);
              }
              try
              {
                  context.SaveChanges();
              }
              catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
              {
                  var innerException = ex.InnerException;
                  // Examina la innerException para obtener más detalles sobre el error
                  throw;
              }

        }
    }
}