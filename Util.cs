using System.Device.Gpio;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using System.IdentityModel.Tokens;

//
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
//
namespace nscore;
//using System.Collections.Generic;

public class Util
{
    //public static bool isStartApp { get; set; }
    public static string WebRootPath { get; set; }

    public static string HolaMundo()
    {
        //
        //  ServoController device = new ServoController();
        //  device.logica();

        // 


        ObserverCoordinates ciudad = ObserverCoordinates.cityRosario;//ObserverCoordinates.cityQuito  cityRosario
        //double julianDateGreenwich = AstronomyEngine.GetJulianDate();
        double siderealTime_local = AstronomyEngine.GetTSL(DateTime.UtcNow, ciudad);
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
            log(e);
            //DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), e, DateTime.Now);
            //Console.WriteLine(e.Message);
        }
        return l;
    }
    public static List<Log> getLogs()
    {
        List<Log> result = new List<Log>();
        try
        {
            using (var context = new AstroDbContext())
            {
                result = context.Logs.ToList();
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static void log(Exception pEx)
    {
        Console.WriteLine(pEx.Message);
        Log log = new Log(pEx);
        try
        {
            using (var context = new AstroDbContext())
            {
                context.Logs.Add(log);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
            log_file(new Log(ex));
            log_file(log);
        }
    }
    public static void saveDHT(double Temperature, double Humidity, string pNameIoT, string chipID)
    {
        DHT o = new DHT();
        o.PublicID = Guid.NewGuid();
        o.Temperature = Temperature;
        o.Humidity = Humidity;
        o.chipID = chipID;
        o.NameIoT = pNameIoT;
        o.fecha = DateTime.Now;
        ProcessESP8266.saveDHT(o);
    }
    public static List<DHT> getDHT()
    {
        return ProcessESP8266.getDHT();
    }
    public static void log_file(Log pEx)
    {
        try
        {
            string nombreFile = pEx.fecha.Year.ToString("0000") + pEx.fecha.Month.ToString("00") + pEx.fecha.Day.ToString("00") + Helper.app + ".txt";
            string path = Path.Combine(Helper.folder, "log");
            var sb = new System.Text.StringBuilder();
            sb.Append(pEx.Message);
            sb.Append(Environment.NewLine);
            if (pEx.InnerException != null)
            {
                sb.Append(pEx.InnerException);
                sb.Append(Environment.NewLine);
            }
            if (!string.IsNullOrEmpty(pEx.StackTrace))
            {
                sb.Append(pEx.StackTrace);
                sb.Append(Environment.NewLine);
            }
            if (!string.IsNullOrEmpty(pEx.info))
            {
                sb.Append(pEx.info);
                sb.Append(Environment.NewLine);
            }
            sb.Append(pEx.fecha.ToString());
            sb.Append(Environment.NewLine);
            sb.Append(pEx.app);
            string strLine = sb.ToString();
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
            string FilePath = Path.Combine(path, nombreFile);
            if (!File.Exists(FilePath))
            {
                using (StreamWriter sw = File.CreateText(FilePath))
                {
                    sw.WriteLine(strLine);
                    sw.Close();
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(FilePath))
                {
                    sw.WriteLine(strLine);
                    sw.Close();
                }
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine(ex);
        }
    }
    /* public static List<AstronomicalObject> getAstronomicalObjects()
     {
         List<AstronomicalObject> result = new List<AstronomicalObject>();
         try
         {
             using (var context = new AstroDbContext())
             {
                 result = context.AstronomicalObjects.ToList();
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /* public static List<AstronomicalObject> CargaInicialAstronomicalObject(bool pIsSaveBD = true)
     {
         List<AstronomicalObject> result = new List<AstronomicalObject>();
         try
         {
             System.Data.DataTable oDataTable = ProcessExcel.GetDataTableAstronomy();
             List<string> oListString = ProcessFile.GetListStringAstronomy("simbadEstrellas.csv");
             char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
             foreach (System.Data.DataRow oRow in oDataTable.Rows)
             {
                 if (oRow["5"] != DBNull.Value)
                 {
                     AstronomicalObject o = new AstronomicalObject();
                     o.nameLatin = oRow["5"].ToString();
                     o.name = o.nameLatin;
                     if (!string.IsNullOrEmpty(o.nameLatin) && oListString.FindAll(er => er.Contains(o.nameLatin)).Count > 0)
                     {

                         List<string> oAux = oListString.FindAll(er => er.Contains(o.nameLatin));
                         if (oAux.Count == 1)
                         {
                             string oLine = oAux.FirstOrDefault();
                             string[] words = oLine.Split(',');

                             o.simbadOID = Convert.ToInt32(words[0]);

                             o.ra = Convert.ToDouble(words[1].Replace(".", systemSeparator.ToString()));
                             //o.ra = Convert.ToDouble(words[1]);
                             //o.dec = Convert.ToDouble(words[2]);
                             o.dec = Convert.ToDouble(words[2].Replace(".", systemSeparator.ToString()));
                             o.simbadNameDefault = words[3];
                             o.simbadNames = words[4];

                             string[] names = words[4].Split('|');
                             foreach (string oName in names)
                             {
                                 if (("|" + oName).Contains("|HD "))
                                 {
                                     string nroHD = oName.Replace("HD", "");
                                     int n;
                                     bool isNumeric = int.TryParse(nroHD, out n);
                                     if (isNumeric)
                                     {
                                         o.idHD = Convert.ToInt32(nroHD);
                                         break;
                                     }
                                 }
                             }
                         }
                         else if (oAux.Count > 1)
                         {
                             o.simbadNameDefault = o.nameLatin;
                         }
                         else
                         {
                             //throw new Exception("No deberia pasar por aca");
                         }
                     }
                     if (pIsSaveBD)
                     {
                         using (var context = new AstroDbContext())
                         {
                             context.AstronomicalObjects.Add(o);
                             context.SaveChanges();
                         }
                     }
                     result.Add(o);
                 }
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /* public static List<AstronomicalObject> CargaInicialAstronomicalObject_HD(bool pIsSaveBD = true)
     {
         List<AstronomicalObject> result = new List<AstronomicalObject>();
         try
         {
             result = getAstronomicalObjects().Where(x => x.idHD != null && x.ra == null && x.dec == null).ToList();
             //System.Data.DataTable oDataTable = ProcessExcel.GetDataTableAstronomy();
             List<string> oListString = ProcessFile.GetListStringAstronomy("simbadEstrellas_falta.csv");
             char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
             foreach (AstronomicalObject oAstro in result)
             {

                 if (oAstro.idHD != null && oListString.FindAll(er => er.Contains(oAstro.idHD.ToString())).Count > 0)
                 {

                     List<string> oAux = oListString.FindAll(er => er.Contains(oAstro.idHD.ToString()));
                     if (oAux.Count == 1)
                     {
                         string oLine = oAux.FirstOrDefault();
                         string[] words = oLine.Split(',');

                         oAstro.simbadOID = Convert.ToInt32(words[0]);

                         oAstro.ra = Convert.ToDouble(words[1].Replace(".", systemSeparator.ToString()));
                         oAstro.dec = Convert.ToDouble(words[2].Replace(".", systemSeparator.ToString()));
                         oAstro.simbadNameDefault = words[3];
                         oAstro.simbadNames = words[4];
                         if (pIsSaveBD)
                         {
                             using (var context = new AstroDbContext())
                             {
                                 context.Update(oAstro);
                                 //context.AstronomicalObjects.Add(oAstro);
                                 context.SaveChanges();
                             }
                         }

                     }
                     else
                     {
                         //throw new Exception("No deberia pasar por aca");
                     }
                 }
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /* public static AstronomicalObject UpdateAstronomicalObject_HD_onlyIdHD(string nameLatin, int idHD)
     {
         AstronomicalObject result = null;
         try
         {
             result = getAstronomicalObjects().Where(x => x.nameLatin == nameLatin).FirstOrDefault();
             if (result != null)
             {
                 result.idHD = idHD;
                 using (var context = new AstroDbContext())
                 {
                     context.Update(result);
                     context.SaveChanges();
                 }
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /* public static string UpdateAstronomicalObject_HD_All()
     {

         CargaInicialAstronomicalObject_HD(true);
         return "Ok";
     }*/
    /* public static List<AstronomicalObject> restoreAstronomicalObjects()
     {
         List<AstronomicalObject> l = getAstronomicalObjects_fileLoad();
         using (var context = new AstroDbContext())
         {
             foreach (AstronomicalObject oFila in l)
             {
                 context.AstronomicalObjects.Add(oFila);
             }
             try
             {
                 context.SaveChanges();
             }
             catch (Exception ex)
             {
                 log(ex);
             }
         }
         return l;
     }*/
    public static string getAstronomicalObjects_copia()
    {
        string result = string.Empty;
        List<AstronomicalObject> l_AstronomicalObject = getAstronomicalObjects_fileLoad();
        try
        {

            char systemSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            System.Data.DataTable tb = ProcessExcel.GetDataTableAstronomy();
            foreach (System.Data.DataRow oRow in tb.Rows)
            {
                string name = oRow["3"].ToString();//oRow["5"].ToString().Replace(" en SIMBAD.", "");
                AstronomicalObject o = l_AstronomicalObject.Where(x => x.name == name).FirstOrDefault();
                if (null != o)
                {
                    //o.nameLatin = o.name;
                    //o.name = oRow["3"].ToString();
                    o.bayerDesignation = oRow["2"].ToString();

                    string numeroTexto = oRow["1"].ToString().Replace(" var", string.Empty).Replace(".", systemSeparator.ToString()).ToString();
                    double? nroDouble = null;
                    if (numeroTexto.Contains("+"))
                    {
                        nroDouble = Convert.ToDouble(numeroTexto.Replace("+", string.Empty));
                    }
                    else if (numeroTexto.Contains("−"))
                    {
                        nroDouble = Convert.ToDouble(numeroTexto.Replace("−", string.Empty)) * -1.0;
                    }
                    else
                    {
                        nroDouble = Convert.ToDouble(numeroTexto);
                    }
                    o.magnitudAparente = nroDouble;
                }
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        string json = System.Text.Json.JsonSerializer.Serialize(l_AstronomicalObject);
        string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "estrellaCopiaSeguridad.json");
        File.WriteAllText(pathAstronomy, json);
        result = json;
        return result;
    }


    /* public static string getAstronomicalObjects_fileSave()
     {
         try
         {
             using (var context = new AstroDbContext())
             {
                 string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "estrellaCopiaSeguridad.json");
                 string json = System.Text.Json.JsonSerializer.Serialize(context.AstronomicalObjects.ToList());
                 File.WriteAllText(pathAstronomy, json);
                 return json;
             }

         }
         catch (Exception ex)
         {
             log(ex);
         }
         return null;
     }*/
    public static List<AstronomicalObject> getAstronomicalObjects_fileLoad()
    {
        try
        {
            string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "estrellaCopiaSeguridad.json");
            string json = File.ReadAllText(pathAstronomy);
            List<AstronomicalObject> instancia = System.Text.Json.JsonSerializer.Deserialize<List<AstronomicalObject>>(json);
            return instancia;
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return null;
    }
    /* public static List<Constellation> CargaInicialConstelación(bool pIsSaveBD = true)
     {

         List<Constellation> result = new List<Constellation>();
         try
         {
             System.Data.DataTable oDataTable = ProcessExcel.GetDataTableConstelaciones();
             // int id = 0;
             foreach (System.Data.DataRow oRow in oDataTable.Rows)
             {
                 // id++;
                 // if (oRow["0"] != DBNull.Value)
                 //{
                 Constellation o = new Constellation();
                 o.id = Convert.ToInt32(oRow["0"].ToString());
                 o.nameLatin = oRow["1"].ToString();
                 o.name = oRow["2"].ToString();
                 o.abbreviation = oRow["3"].ToString();
                 o.Genitivo = oRow["4"].ToString();
                 o.Origen = oRow["5"].ToString();
                 o.DescritaPor = oRow["6"].ToString();
                 o.Extension = Convert.ToDouble(oRow["7"].ToString().Replace(".", ","));
                 o.ra = Convert.ToDouble(oRow["8"].ToString().Replace(".", ","));
                 o.dec = Convert.ToDouble(oRow["9"].ToString().Replace(".", ","));
                 o.visible = true;
                 result.Add(o);
                 //   }
             }
             if (pIsSaveBD)
             {
                 using (var context = new AstroDbContext())
                 {
                     context.Constellations.AddRange(result);
                     context.SaveChanges();
                 }
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /* public static List<Constellation> getConstelaciones()
     {
         List<Constellation> result = new List<Constellation>();
         try
         {
             using (var context = new AstroDbContext())
             {
                 result = context.Constellations.ToList();
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /*public static List<Star> getAllStars()
    {
        List<Star> result = new List<Star>();
        List<AstronomicalObject> l = nscore.Util.getAstronomicalObjects().Where(x => x.magnitudAparente != null).OrderBy(x => x.magnitudAparente).ToList();
        //foreach (AstronomicalObject oStar in l){
        for (int i = 0; i < l.Count; i++)
        {
            AstronomicalObject o = l[i];
            Star oStar = new Star();
            oStar.id = o.idHD;
            oStar.dec = o.dec.Value;
            oStar.ra = o.ra.Value;
            oStar.name = o.getName();
            oStar.hip = o.idHD;
            result.Add(oStar);
        }
        return result;
    }*/
    /* public static string updateConstelacion(int id, int idHD, string pName)
     {
         string result = "!Ok";
         try
         {
             using (var context = new AstroDbContext())
             {
                 Constellation o = context.Constellations.Where(x => x.id == id).FirstOrDefault();
                 if (o != null)
                 {
                     o.idHD_startRef = idHD;
                     if (!string.IsNullOrEmpty(pName))
                     { o.name = pName; }
                     context.Constellations.Update(o);
                     context.SaveChanges();
                     result = "Ok";
                 }

             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /*public static string fileSave_Constelaciones()
    {
        try
        {
            using (var context = new AstroDbContext())
            {
                string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "Constellations.json");
                string json = System.Text.Json.JsonSerializer.Serialize(context.Constellations.ToList());
                File.WriteAllText(pathAstronomy, json);
                return json;
            }

        }
        catch (Exception ex)
        {
            log(ex);
        }
        return null;
    }*/
    public static List<Constellation> fileLoad_Constelaciones()
    {
        try
        {
            string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "Constellations.json");
            string json = File.ReadAllText(pathAstronomy);
            List<Constellation> instancia = System.Text.Json.JsonSerializer.Deserialize<List<Constellation>>(json);
            return instancia;
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return null;
    }
    /*  public static List<Constellation> restoreConstelaciones()
      {
          List<Constellation> l = fileLoad_Constelaciones();
          using (var context = new AstroDbContext())
          {
              foreach (Constellation oFila in l)
              {
                  context.Constellations.Add(oFila);
              }
              try
              {
                  context.SaveChanges();
              }
              catch (Exception ex)
              {
                  log(ex);
              }
          }
          return l;
      }*/

    public static async Task<string> restoreUser()
    {
        string result = string.Empty;
        try
        {
            using (var context = new AstroDbContext())
            {
                User o = new User();
                o.name = nscore.Helper.user_name;
                o.login = nscore.Helper.user_name;
                string pass = Convert.ToBase64String(Cryptography.ComputeHash(Encoding.UTF8.GetBytes(nscore.Helper.user_pass)));
                o.pass = pass;
                context.Users.Add(o);
                context.SaveChanges();
            }
            result = "Ok";
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<string> restore()
    {
        //restoreConstelaciones();
        //restoreAstronomicalObjects();
        await restoreUser();
        restoreDatosConfig();
        await restoreStellarium();
        return "Ok";
    }
    public static string restoreDatosConfig()
    {
        string result = string.Empty;
        try
        {

            ConfigAnt oConfigAnt = ConfigAnt.configDefault;
            using (var context = new AstroDbContext())
            {
                // Obtener el tipo del objeto
                Type tipoObjeto = oConfigAnt.GetType();

                // Obtener todas las propiedades del objeto
                System.Reflection.PropertyInfo[] propiedades = tipoObjeto.GetProperties();

                // Recorrer las propiedades e imprimir sus nombres y valores
                foreach (var propiedad in propiedades)
                {
                    object valor = propiedad.GetValue(oConfigAnt);
                    if (valor != null)
                    {
                        if (valor is double)
                        {
                            context.Configs.Add(new Config() { name = propiedad.Name, valueDouble = Convert.ToDouble(valor) });
                        }
                        if (valor is int)
                        {
                            context.Configs.Add(new Config() { name = propiedad.Name, valueInt = Convert.ToInt32(valor) });
                        }
                        if (valor is String || valor is Guid)
                        {
                            context.Configs.Add(new Config() { name = propiedad.Name, value = Convert.ToString(valor) });
                        }

                    }

                }
                context.Configs.Add(new Config() { name = "servoH", valueDouble = 0 });
                context.Configs.Add(new Config() { name = "servoV", valueDouble = 0 });
                context.Configs.Add(new Config() { name = "esp32", valueInt = 0 });
                context.Configs.Add(new Config() { name = "esp32_activo", valueInt = 0 });
                context.SaveChanges();
                result = "Ok";
            }
        }
        catch (Exception ex)
        {
            result = "!Ok";
            log(ex);
        }
        return result;
    }
    /*public static string AsignarConstelacionAEstrellas()
    {
        string result = string.Empty;
        List<Constellation> l_Constellation = new List<Constellation>();
        List<AstronomicalObject> l_AstronomicalObject = new List<AstronomicalObject>();
        try
        {
            using (var context = new AstroDbContext())
            {
                l_Constellation = context.Constellations.ToList();
                l_AstronomicalObject = context.AstronomicalObjects.ToList();
                foreach (Constellation oItem in l_Constellation)
                {
                    List<AstronomicalObject> l_AstronomicalObject_temp = l_AstronomicalObject.Where(x => x.nameLatin.Contains(oItem.Genitivo)).ToList();
                    int cant = l_AstronomicalObject_temp.Count;
                    result += " { ";
                    result += "nombre: " + oItem.name + " - Cantidad: " + cant;
                    foreach (AstronomicalObject oItem_detalle in l_AstronomicalObject_temp)
                    {
                        result += " [ " + oItem_detalle.name + " ] ";
                    }
                    result += " }";
                }
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }*/
    public static List<AntTracking> getAntTrackings()
    {
        List<AntTracking> result = new List<AntTracking>();
        try
        {
            using (var context = new AstroDbContext())
            {
                result = context.AntTrackings.ToList();
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<List<StellariumStar_base>> getInfoStellarium()
    {
        List<StellariumStar_base> result = new List<StellariumStar_base>();
        // Astronomical_stellarium
        using var httpClient = new HttpClient();

        try
        {
            List<AstronomicalObject> l = nscore.Util.getAstronomicalObjects_fileLoad().Where(x => x.magnitudAparente != null).OrderBy(x => x.magnitudAparente).ToList();
            for (int i = 0; i < l.Count; i++)
            {

                string url = "http://localhost:8090/api/objects/info?name=" + "HD" + l[i].idHD.ToString() + "&format=json";
                HttpResponseMessage response = await httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                StellariumStar_base data = await response.Content.ReadFromJsonAsync<StellariumStar_base>();
                result.Add(data);
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<List<StellariumStar>> Stellarium_CompletarDatos()
    {
        List<StellariumStar> l_result = new List<StellariumStar>();
        List<StellariumStar_base> l = await getInfoStellarium();
        List<AstronomicalObject> l_AstronomicalObjects = nscore.Util.getAstronomicalObjects_fileLoad().Where(x => x.magnitudAparente != null).OrderBy(x => x.magnitudAparente).ToList();
        foreach (StellariumStar_base oStellarium in l)
        {
            StellariumStar oAstronomical_stellarium = CopiarPropiedades(oStellarium, new StellariumStar());
            int hip = Convert.ToInt32(oStellarium.name.ToString().Replace("HIP ", ""));
            string name_hip = "HIP" + hip.ToString().PadLeft(7);// "HIP  71683";
            AstronomicalObject oAstronomicalObject = l_AstronomicalObjects.Where(x => x.simbadNames.Contains(name_hip)).FirstOrDefault();
            if (oAstronomicalObject != null)
            {
                oAstronomical_stellarium.name_web = oAstronomicalObject.name;
            }
            oAstronomical_stellarium.hip = hip;
            l_result.Add(oAstronomical_stellarium);
        }
        return l_result;
    }
    public static async Task<List<StellariumStar>> restoreStellarium()
    {
        List<StellariumStar> l = getAstronomical_stellarium_fileLoad();
        using (var context = new AstroDbContext())
        {
            foreach (StellariumStar oFila in l)
            {
                context.StellariumStars.Add(oFila);
            }
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                log(ex);
            }
        }
        return l;
    }
    public static StellariumStar CopiarPropiedades(StellariumStar_base padre, StellariumStar hija)
    {
        System.Reflection.PropertyInfo[] propiedadesPadre = typeof(StellariumStar_base).GetProperties();
        System.Reflection.PropertyInfo[] propiedadesHija = typeof(StellariumStar).GetProperties();

        foreach (var propiedadPadre in propiedadesPadre)
        {
            foreach (var propiedadHija in propiedadesHija)
            {
                if (propiedadPadre.Name == propiedadHija.Name &&
                    propiedadPadre.PropertyType == propiedadHija.PropertyType &&
                    propiedadPadre.CanRead && propiedadHija.CanWrite)
                {
                    propiedadHija.SetValue(hija, propiedadPadre.GetValue(padre));
                    break;
                }
            }
        }
        return hija;
    }
    public static List<StellariumStar> getAstronomical_stellarium()
    {
        List<StellariumStar> result = new List<StellariumStar>();
        try
        {
            using (var context = new AstroDbContext())
            {
                result = context.StellariumStars.ToList();
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static List<StellariumStar> getAstronomical_stellarium_fileLoad()
    {
        try
        {
            string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "Astronomical_stellarium.json");
            string json = File.ReadAllText(pathAstronomy);
            List<StellariumStar> instancia = System.Text.Json.JsonSerializer.Deserialize<List<StellariumStar>>(json);
            return instancia;
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return null;
    }
    public static List<Star> getAllStars_stellarium()
    {
        List<Star> result = new List<Star>();
        List<StellariumStar> l = getAstronomical_stellarium();//.Where(x => x.absolute_mag != null).OrderBy(x => x.absolute_mag).ToList();
        //foreach (AstronomicalObject oStar in l){
        for (int i = 0; i < l.Count; i++)
        {
            StellariumStar o = l[i];
            Star oStar = new Star();
            oStar.id = o.hip.Value;
            oStar.dec = o.dec.Value;
            oStar.ra = o.ra.Value;
            oStar.name = o.getName();
            oStar.hip = o.hip.Value; // Revisar
            result.Add(oStar);
        }
        return result;
    }
    public static async Task<string> Astronomical_stellarium_copia()
    {
        string result = string.Empty;
        List<StellariumStar> l = await Stellarium_CompletarDatos();
        string json = System.Text.Json.JsonSerializer.Serialize(l);
        string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "Astronomical_stellarium.json");
        File.WriteAllText(pathAstronomy, json);
        result = json;
        return result;
    }
    public static async Task<string> esp32_set(int pValue)
    {
        string result = string.Empty;
        try
        {
            using (var context = new AstroDbContext())
            {

                Config o = context.Configs.Where(x => x.name == "esp32").FirstOrDefault();
                if (o != null)
                {
                    o.valueInt = pValue;
                    context.SaveChanges();
                }

                result = "Ok";
            }
        }
        catch (Exception ex)
        {
            result = "!Ok";
            log(ex);
        }
        return result;
    }
    public static async Task<int> esp32_get()
    {
        int result = -1;
        try
        {
            using (var context = new AstroDbContext())
            {

                Config o = context.Configs.Where(x => x.name == "esp32").FirstOrDefault();
                if (o != null)
                {
                    result = o.valueInt != null ? o.valueInt.Value : -1;
                }

            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<int> esp32_util(int pValue)
    {
        int result = -1;
        if (pValue == 0 || pValue == 1)
        {
            result = await esp32_set(pValue) == "Ok" ? pValue : -1;
        }
        else
        {
            result = await esp32_get();
        }
        return result;
    }
    public static async Task<string> esp32_activo_set(int pValue)
    {
        string result = string.Empty;
        try
        {
            using (var context = new AstroDbContext())
            {

                Config o = context.Configs.Where(x => x.name == "esp32_activo").FirstOrDefault();
                if (o != null)
                {
                    o.valueInt = pValue;
                    context.SaveChanges();
                }

                result = "Ok";
            }
        }
        catch (Exception ex)
        {
            result = "!Ok";
            log(ex);
        }
        return result;
    }
    public static async Task<int> esp32_activo_get()
    {
        int result = -1;
        try
        {
            using (var context = new AstroDbContext())
            {

                Config o = context.Configs.Where(x => x.name == "esp32_activo").FirstOrDefault();
                if (o != null)
                {
                    result = o.valueInt != null ? o.valueInt.Value : -1;
                }

            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<int> SessionApp_inicio()
    {
        try
        {
            using (var context = new AstroDbContext())
            {
                SessionApp o = new SessionApp();
                o.publicID = Singleton_SessionApp.Instance.publicID;
                o.name = "Señalador de Estrellas";
                o.createDate = DateTime.Now;
                context.SessionApps.Add(o);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return 0;
    }
    public static async Task<int> inicioApp()
    {
        //await esp32_activo_set(0);
        //isStartApp = true;
        await SessionApp_inicio();
        return 0;
    }
    /*public static async Task<bool> AntTrackingStatus(Guid pGuid, string pEstado, Guid? pSessionDevice_publicID)
    {
        bool result = false;
        using (var context = new AstroDbContext())
        {
            AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();
            if (oAntTracking != null)
            {
                oAntTracking.status = pEstado;
                oAntTracking.statusUpdateDate = DateTime.Now;
                if (pSessionDevice_publicID != null)
                {
                    oAntTracking.sessionDevice_publicID = pSessionDevice_publicID.Value;
                }
                context.SaveChanges();
                result = true;
            }
        }
        return result;
    }*/
    /* public static async Task<Esp32_astro> esp32_getAstro_movedServo()
     {
         Esp32_astro result = null;
         try
         {
             Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
             using (var context = new AstroDbContext())
             {
                 AntTracking oAntTracking = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_movingServo).OrderBy(x1 => x1.date).FirstOrDefault();
                 if (oAntTracking != null)
                 {
                     await AntTrackingStatus(oAntTracking.publicID, Constantes.astro_status_movedServo, Guid.NewGuid());
                 }
                 result = new Esp32_astro()
                 {
                     publicID = oAntTracking.publicID,
                     horizontal_grados = oAntTracking.h == null ? 0 : oAntTracking.h.Value,
                     vertical_grados = oAntTracking.v == null ? 0 : oAntTracking.v.Value
                 };
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /* public static async Task<Esp32_astro> esp32_getAstro()
     {
         Esp32_astro result = null;
         try
         {
             using (var context = new AstroDbContext())
             {
                 Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
                 AntTracking oAntTracking = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_calculationResolution).OrderBy(x1 => x1.date).FirstOrDefault();
                 if (oAntTracking != null)
                 {
                     AntTracking oAntTracking_ant = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_movedServo && x.statusUpdateDate != null).OrderByDescending(x1 => x1.statusUpdateDate.Value).FirstOrDefault();
                     double? h_old = null;
                     double? v_old = null;
                     double? h_diferencia_grados = null;
                     double? v_diferencia_grados = null;
                     if (oAntTracking_ant != null)
                     {
                         h_old = oAntTracking_ant.h;
                         v_old = oAntTracking_ant.v;
                         if (h_old != null && oAntTracking.h != null)
                         {
                             if (h_old.Value > oAntTracking.h.Value)
                             {
                                 h_diferencia_grados = Math.Abs(h_old.Value - oAntTracking.h.Value);
                             }
                             else
                             {
                                 h_diferencia_grados = Math.Abs(oAntTracking.h.Value - h_old.Value);
                             }
                         }
                         if (v_old != null && oAntTracking.v != null)
                         {
                             if (v_old.Value > oAntTracking.v.Value)
                             {
                                 v_diferencia_grados = Math.Abs(v_old.Value - oAntTracking.v.Value);
                             }
                             else
                             {
                                 v_diferencia_grados = Math.Abs(oAntTracking.v.Value - v_old.Value);
                             }
                         }
                     }
                     double h_sleep_secs = Constantes.servo_sleep_max;
                     double v_sleep_secs = Constantes.servo_sleep_max;
                     if (h_diferencia_grados != null)
                     {
                         h_sleep_secs = double.Round((h_sleep_secs * h_diferencia_grados.Value) / 180.0, 1);
                         if (h_sleep_secs < Constantes.servo_sleep_min)
                         {
                             h_sleep_secs = 0.5;
                         }
                     }
                     if (v_diferencia_grados != null)
                     {
                         v_sleep_secs = double.Round((v_sleep_secs * v_diferencia_grados.Value) / 180.0, 1);
                         if (v_sleep_secs < Constantes.servo_sleep_min)
                         {
                             v_sleep_secs = 0.5;
                         }
                     }

                     await AntTrackingStatus(oAntTracking.publicID, Constantes.astro_status_movingServo, null);
                     result = new Esp32_astro()
                     {
                         type = oAntTracking.type,
                         isLaser = oAntTracking.isLaser,
                         publicID = oAntTracking.publicID,
                         horizontal_grados = oAntTracking.h == null ? 0 : oAntTracking.h.Value,
                         vertical_grados = oAntTracking.v == null ? 0 : oAntTracking.v.Value,
                         horizontal_grados_ant = h_old,
                         vertical_grados_ant = v_old,
                         horizontal_grados_sleep = h_sleep_secs,
                         vertical_grados_sleep = v_sleep_secs

                     };
                 }
             }
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    /*public static async Task<string> esp32_setAstro(string pPublicID, string pSessionDevice_publicID)
    {
        string result = string.Empty;
        try
        {
            Guid publicID = new Guid(pPublicID);
            Guid sessionDevice_publicID = new Guid(pSessionDevice_publicID);
            await AntTrackingStatus(publicID, Constantes.astro_status_movedServo, sessionDevice_publicID);
            result = "Ok";
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }*/
    public static Guid newAstroTracking(string pType, string pDevice_name, double pRa_h, double pDec_v, double? pH_calibrate = null, double? pV_calibrate = null)
    {
        Guid oGuid = Guid.NewGuid();
        using (var context = new AstroDbContext())
        {

            nscore.AntTracking o = new nscore.AntTracking(oGuid, pType, pDevice_name, pRa_h, pDec_v);
            if (pH_calibrate != null)
            {
                o._h_calibrate = pH_calibrate;
            }
            if (pV_calibrate != null)
            {
                o._v_calibrate = pV_calibrate;
            }
            context.AntTrackings.Add(o);
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                nscore.Util.log(ex);
            }
        }
        return oGuid;
    }
    public static Guid newAstroTracking_laser(string pType, int pIsLaser, string pDevice_name)
    {
        Guid oGuid = Guid.NewGuid();
        using (var context = new AstroDbContext())
        {

            nscore.AntTracking o = new nscore.AntTracking(oGuid, pType, pIsLaser, pDevice_name);
            context.AntTrackings.Add(o);
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                nscore.Util.log(ex);
            }
        }
        return oGuid;
    }
    /* public static async Task<Guid> sessionDeviceAdd(string pDevice_publicID, string pDevice_name)
     {
         Guid result = Guid.Empty;
         try
         {
             Guid device_publicID = new Guid(pDevice_publicID);
             Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
             using (var context = new AstroDbContext())
             {
                 SessionDevice o = new SessionDevice();
                 o.device_name = pDevice_name;
                 o.device_publicID = device_publicID;
                 o.sessionApp_publicID = sessionApp_publicID;
                 o.createDate = DateTime.Now;
                 result = o.publicID;
                 context.SessionDevices.Add(o);
                 context.SaveChanges();
             }
             Guid newAntTracking_inicio = await antTracking_resetSession(result.ToString());
         }
         catch (Exception ex)
         {
             log(ex);
         }
         return result;
     }*/
    public static async Task<string> isSessionDeviceOk(string pSessionDevice_publicID)
    {
        string result = "!Ok";
        try
        {
            Guid sessionDevice_publicID = new Guid(pSessionDevice_publicID);
            Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
            using (var context = new AstroDbContext())
            {
                SessionDevice o = context.SessionDevices.Where(x => x.publicID == sessionDevice_publicID).FirstOrDefault();//&& x.sessionApp_publicID == sessionApp_publicID
                if (o != null && o.sessionApp_publicID == sessionApp_publicID)
                {
                    result = "Ok";
                }
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    /*public static async Task<Guid> antTracking_resetSession(string pSessionDevice_publicID)
    {
        Guid result = Guid.Empty;
        try
        {
            Guid sessionDevice_publicID = new Guid(pSessionDevice_publicID);
            using (var context = new AstroDbContext())
            {
                SessionDevice oSessionDevices = context.SessionDevices.Where(x => x.publicID == sessionDevice_publicID).FirstOrDefault();
                if (oSessionDevices != null)
                {
                    Guid sessionApp_publicID = oSessionDevices.sessionApp_publicID;
                    List<nscore.AntTracking> l = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID).ToList();
                    DateTime dateNow = DateTime.Now;
                    foreach (var oItem in l)
                    {
                        oItem.status = Constantes.astro_status_resetSession;
                        oItem.statusUpdateDate = dateNow;
                    }
                    context.SaveChanges();
                    Guid publicID = newAstroTracking(Constantes.astro_type_servoAngle_inicio, 0, 0);
                    result = publicID;
                }
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }*/
    public static async Task<List<User>> getUsers()
    {
        List<User> result = new List<User>();
        try
        {
            using (var context = new AstroDbContext())
            {
                result = context.Users.ToList();

            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<Guid> newSessionUser(Guid pUser_publicID)
    {
        Guid result = Guid.Empty;
        try
        {
            using (var context = new AstroDbContext())
            {
                SessionUser o = new SessionUser();
                o.user_publicID = pUser_publicID;
                context.SessionUsers.Add(o);
                result = o.publicID;
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
    public static async Task<string> login(request_User pUser)
    {
        string result = string.Empty;
        try
        {
            bool isLogin = false;
            User oUser = getUsers().Result.Where(x => x.login == pUser.name).FirstOrDefault();
            if (oUser != null)
            {
                isLogin = Cryptography.VerifyHash(Encoding.UTF8.GetBytes(pUser.pass), Convert.FromBase64String(oUser.pass));
            }
            if (oUser != null && isLogin)
            {
                var issuer = nscore.Helper.Jwt_Issuer;
                var audience = nscore.Helper.Jwt_Audience;
                var key = Encoding.ASCII.GetBytes(nscore.Helper.Jwt_Key);
                var securityKey = new SymmetricSecurityKey(key);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(new[]
                    {
                new System.Security.Claims.Claim("Id", Guid.NewGuid().ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Name, oUser.name),
                  new System.Security.Claims.Claim("user_publicID", oUser.publicID.ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(5),
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
                };
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);
                var stringToken = tokenHandler.WriteToken(token);
                result = stringToken;
                await newSessionUser(oUser.publicID);
            }
        }
        catch (Exception ex)
        {
            log(ex);
        }
        return result;
    }
}