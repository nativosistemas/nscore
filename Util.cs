using DocumentFormat.OpenXml.Vml.Spreadsheet;

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
                log(ex);
            }

        }
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
    public static List<AstronomicalObject> getAstronomicalObjects()
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
    }
    public static List<AstronomicalObject> CargaInicialAstronomicalObject(bool pIsSaveBD = true)
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
    }
    public static List<AstronomicalObject> CargaInicialAstronomicalObject_HD(bool pIsSaveBD = true)
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
    }
    public static AstronomicalObject UpdateAstronomicalObject_HD_onlyIdHD(string nameLatin, int idHD)
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
    }
    public static string UpdateAstronomicalObject_HD_All()
    {
        /* UpdateAstronomicalObject_HD_onlyIdHD("Rigel", 34085);
         UpdateAstronomicalObject_HD_onlyIdHD("Capella A", 34029);
         UpdateAstronomicalObject_HD_onlyIdHD("Deneb", 197345);
         UpdateAstronomicalObject_HD_onlyIdHD("Beta Crucis", 111123);
         UpdateAstronomicalObject_HD_onlyIdHD("Acrux A", 108248);
         UpdateAstronomicalObject_HD_onlyIdHD("Adara", 52089);
         UpdateAstronomicalObject_HD_onlyIdHD("Gamma Crucis", 108903);
         UpdateAstronomicalObject_HD_onlyIdHD("Beta Carinae", 80007);
         UpdateAstronomicalObject_HD_onlyIdHD("Alnitak A", 37742);
         UpdateAstronomicalObject_HD_onlyIdHD("Alfa Gruis", 209952);
         UpdateAstronomicalObject_HD_onlyIdHD("Gamma2 Velorum", 68273);
         UpdateAstronomicalObject_HD_onlyIdHD("Theta Scorpii", 159532);
         UpdateAstronomicalObject_HD_onlyIdHD("Alfa Trianguli Australis", 150798);
         UpdateAstronomicalObject_HD_onlyIdHD("Delta Velorum", 74956);
         UpdateAstronomicalObject_HD_onlyIdHD("Beta Ceti", 4128);
         UpdateAstronomicalObject_HD_onlyIdHD("Theta Centauri", 123139);
         UpdateAstronomicalObject_HD_onlyIdHD("Mirach", 6860);
         UpdateAstronomicalObject_HD_onlyIdHD("Acrux B", 108249);
         UpdateAstronomicalObject_HD_onlyIdHD("Alfa Ophiuchi", 159561);
         UpdateAstronomicalObject_HD_onlyIdHD("Beta Gruis", 214952);
         UpdateAstronomicalObject_HD_onlyIdHD("Lambda Velorum", 78647);
         UpdateAstronomicalObject_HD_onlyIdHD("Etamin", 164058);
         UpdateAstronomicalObject_HD_onlyIdHD("Sadr", 194093);
         UpdateAstronomicalObject_HD_onlyIdHD("Iota Carinae", 80404);
         UpdateAstronomicalObject_HD_onlyIdHD("Epsilon Centauri", 118716);
         UpdateAstronomicalObject_HD_onlyIdHD("Algieba", 89484);
         UpdateAstronomicalObject_HD_onlyIdHD("Alfa Lupi", 129056);
         UpdateAstronomicalObject_HD_onlyIdHD("Delta Scorpii", 143275);
         UpdateAstronomicalObject_HD_onlyIdHD("Epsilon Scorpii", 151680);
         UpdateAstronomicalObject_HD_onlyIdHD("Eta Centauri", 127973);
         UpdateAstronomicalObject_HD_onlyIdHD("Alfa Phoenicis", 2261);
         UpdateAstronomicalObject_HD_onlyIdHD("Kappa Scorpii", 160578);
         UpdateAstronomicalObject_HD_onlyIdHD("Gamma Cassiopeiae", 5394);
         UpdateAstronomicalObject_HD_onlyIdHD("Eta Canis Majoris", 58350);
         UpdateAstronomicalObject_HD_onlyIdHD("Epsilon Carinae", 71129);
         UpdateAstronomicalObject_HD_onlyIdHD("Kappa Velorum", 81188);
         UpdateAstronomicalObject_HD_onlyIdHD("Epsilon Cygni", 197989);*/
        CargaInicialAstronomicalObject_HD(true);
        return "Ok";
    }
    public static List<AstronomicalObject> RestaurarJsonBD_AstronomicalObjects()
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
    }
    public static string getAstronomicalObjects_copia()
    {
        string result = string.Empty;
        List<AstronomicalObject> l_AstronomicalObject = getAstronomicalObjects_fileLoad();
        try
        {
            /*
            result += "";
            foreach (var i in context.AstronomicalObjects.Where(x => x.idHD != null && x.dec != null && x.ra != null).ToList())
            {
                // Crea un objeto dinámicamente utilizando ExpandoObject
                dynamic dynamicObject = new System.Dynamic.ExpandoObject();

                // Agrega propiedades dinámicamente
                dynamicObject.idHD = i.idHD;
                dynamicObject.name = i.name;
                dynamicObject.dec = i.dec;
                dynamicObject.ra = i.ra;
                dynamicObject.simbadOID = i.simbadOID;
                dynamicObject.simbadNameDefault = i.simbadNameDefault;
                // Agrega el segundo objeto dinámico a la lista
                dynamicList.Add(dynamicObject);}
            }
            */
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


    public static string getAstronomicalObjects_fileSave()
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
    }
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
    public static string Test(nscore.ProcessAnt pProcessAnt)
    {
        DateTime date = new DateTime(2023, 7, 17, 1, 30, 0).ToUniversalTime();
        double siderealTime_local = AstronomyEngine.GetTSL(date, ObserverCoordinates.cityRosario);
        //AstronomyEngine.ConvertirAFechaJuliana_Main();
        //Estrella Vega
        // (J2000.0) 213.90946 / 19.1708 { idHD = 124897, dec = 19.1708 , ra = 213.90946  };
        // (en fecha)  214.18467 / 19.0616  { idHD = 124897, dec =  19.0616, ra = 214.18467 };
        EquatorialCoordinates arturo_eq = new EquatorialCoordinates() { idHD = 124897, dec = 19.0616, ra = 214.18467 };
        // DateTime date = DateTime.UtcNow;
        //DateTime date = new DateTime(2023, 7, 17, 1, 30, 0).ToUniversalTime();
        return pProcessAnt.findStar(date, arturo_eq.idHD, arturo_eq.dec, arturo_eq.ra);
    }
    public static string TestServoPosicionCero(nscore.ProcessAnt pProcessAnt)
    {
        return pProcessAnt.actionAnt_servo(0, 0, 2.5, 12, 2.5, 12, true);
    }
    public static List<Constellation> CargaInicialConstelación(bool pIsSaveBD = true)
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
    }
    public static List<Constellation> getConstelaciones()
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
    }
    public static string fileSave_Constelaciones()
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
    }
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
    public static List<Constellation> restaurarJsonBD_Constelaciones()
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
    }
    public static string restaurarJsonBD()
    {
        restaurarJsonBD_Constelaciones();
        RestaurarJsonBD_AstronomicalObjects();
        return "Ok";
    }
}