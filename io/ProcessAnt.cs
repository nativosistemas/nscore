using System.Diagnostics;

namespace nscore;
public class ProcessAnt : IDisposable
{
    private bool disposedValue = false;
    //private Process _controller = new Process();
    // private bool _versionNew = true;
    private ProcessServo _processServo = new ProcessServo();
    private ProcessServoRango _processServoRango = new ProcessServoRango();
    private ProcessCoordinatesStar _processCoordinatesStar = new ProcessCoordinatesStar();
    private ProcessLaser _processLaser = new ProcessLaser();
    //private Process _controllerLaser = new Process();
    private List<Star> _l_Star = null;
    private List<ObserverCoordinates> _l_City = null;
    private List<Constellation> _l_Constellation = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
    //private static Semaphore _semaphore_ant = new Semaphore(0, 1);
    public ObserverCoordinates city { get { return _city; } set { _city = value; } }

    public double _Horizontal_grados = 0;
    public double _Vertical_grados = 0;
    public double _Horizontal_grados_min = ConfigAnt.configDefault.horizontal_grados_min;//  Math.Round(2.9, 6);
    public double _Horizontal_grados_max = ConfigAnt.configDefault.horizontal_grados_max;//Math.Round(12.7, 6);
    public double _Vertical_grados_min = ConfigAnt.configDefault.vertical_grados_min;//Math.Round(2.5, 6);
    public double _Vertical_grados_max = ConfigAnt.configDefault.vertical_grados_max;//Math.Round(12.2, 6);

    public ProcessAnt()
    {
        _city = getCitys().FirstOrDefault();
        /*_l_Star = new List<Star>();
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
            oStar.idHD = o.idHD;
            _l_Star.Add(oStar);
        }*/
        _l_Star = nscore.Util.getAllStars();
        _l_Constellation = nscore.Util.getConstelaciones();
    }
    public ObserverCoordinates setCity(int id, string pName, double pLatitude, double pLongitude)
    {
        _city = new ObserverCoordinates() { id = id, name = pName, latitude = pLatitude, longitude = pLongitude, altitude = 0 };
        return _city;
    }
    public List<ObserverCoordinates> getCitys()
    {
        if (_l_City == null)
        {
            _l_City = new List<ObserverCoordinates>();
            _l_City.Add(ObserverCoordinates.cityRosario);
            _l_City.Add(ObserverCoordinates.cityQuito);
            _l_City.Add(ObserverCoordinates.cityAtenas);
        }
        return _l_City;
    }
    public string getValoresServos()
    {
        string result = "{ \"horizontal\":" + _Horizontal_grados.ToString(System.Globalization.CultureInfo.InvariantCulture) +
        ", \"vertical\":" + _Vertical_grados.ToString(System.Globalization.CultureInfo.InvariantCulture) +
          ", \"horizontal_min\":" + _Horizontal_grados_min.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", \"horizontal_max\":" + _Horizontal_grados_max.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              ", \"vertical_min\":" + _Vertical_grados_min.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", \"vertical_max\":" + _Vertical_grados_max.ToString(System.Globalization.CultureInfo.InvariantCulture) +
        " }";
        return result;
    }
    public List<Star> getStars()
    {
        double siderealTime_local = AstronomyEngine.GetTSL(DateTime.UtcNow, city);
        foreach (Star oStar in _l_Star)
        {
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
            ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
            if (hc.Altitude > 40)
            {
                oStar.nearZenith = true;
            }
            else
            {
                oStar.nearZenith = false;
            }
            if (hc.Altitude < 1.0)
            {
                oStar.visible = false;
            }
            else
            {
                oStar.visible = true;
            }
            // oStar.visible = true;
        }
        return _l_Star.Where(x => x.visible).ToList();
    }

    public List<Constellation> getConstellations()
    {
        List<Constellation> l = nscore.Util.getConstelaciones();
        double siderealTime_local = AstronomyEngine.GetTSL(DateTime.UtcNow, city);
        foreach (Constellation o in l)
        {
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = o.dec.Value, ra = o.ra.Value };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
            ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
            if (hc.Altitude < 1.0)
            {
                o.visible = false;
            }
            else
            {
                o.visible = true;
                // l.Add(o);
            }
            //o.visible = true;
        }
        return l.Where(x => x.visible).ToList();//
    }
    public string findStar(int pId, bool isNew = false, bool isLaserOn = false)
    {
        string result = string.Empty;
        Star oStar = _l_Star.Where(x => x.id == pId).FirstOrDefault();
        if (oStar != null)
        {
            result = findStar(DateTime.UtcNow, oStar.id, oStar.dec, oStar.ra, isNew, isLaserOn);
        }
        else
        {
            result = "No se encontro estrella";
        }
        return result;
    }
    public string findConstellation(int pId)
    {
        string result = string.Empty;
        Constellation o = _l_Constellation.Where(x => x.id == pId).FirstOrDefault();
        Star oStar = _l_Star.Where(x => x.hip == o.idHD_startRef).FirstOrDefault();
        if (o != null && oStar != null)
        {

            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            // EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            double siderealTime_local = AstronomyEngine.GetTSL(DateTime.UtcNow, city);
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
            if (hc != null)
            {
                ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
                if (oServoCoordinates != null)
                {
                    HorariasCoordinates oHorariasCoordinates = AstronomyEngine.ToHorariasCoordinates(siderealTime_local, eq);
                    string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(eq.ra) + "/" + AstronomyEngine.GetSexagesimal(eq.dec);
                    string strHC = "HA/Dec: " + AstronomyEngine.GetHHmmss(oHorariasCoordinates.HA) + "/" + AstronomyEngine.GetSexagesimal(oHorariasCoordinates.dec);
                    string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(hc.Altitude);
                    result += strEq + "<br/>" + strHC + "<br/>" + strHc + "<br/>";
                    //result += "HD " + eq.idHD.ToString() + "<br/>";
                    result += "Estrella más brillante " + oStar.name + "  -  HD " + o.idHD_startRef.ToString() + "<br/>";
                    result += "Servo: " + moveTheAnt_rango(oServoCoordinates);
                    //moveTheAnt(oServoCoordinates);
                }
                else
                {
                    result = "La estrella de la constelación constelación no es visible";
                }
            }
        }
        else
        {
            result = "No se encontro la estrella de la constelación o no esta cargada";
        }
        return result;
    }
    /*public string findConstellation(DateTime pDate, int pIdHD, double pDec, double pRa)
    {
      string result = string.Empty;
      EquatorialCoordinates eq = new EquatorialCoordinates() { idHD = pIdHD, dec = pDec, ra = pRa };
      result = actionAnt(pDate, eq);
      return result;
    }*/
    public string findStar(DateTime pDate, int pIdHD, double pDec, double pRa, bool isNew = false, bool isLaserOn = false)
    {
        string result = string.Empty;
        EquatorialCoordinates eq = new EquatorialCoordinates() { idHD = pIdHD, dec = pDec, ra = pRa };
        result = actionAnt(pDate, eq, isNew, isLaserOn);
        return result;
    }
    /* public Guid saveAstroTracking(double pRa, double pDec)
     {
         Guid oGuid = Guid.NewGuid();
         using (var context = new AstroDbContext())
         {

             nscore.AstroTracking o = new nscore.AstroTracking(oGuid, pRa, pDec);
             context.AstroTrackings.Add(o);

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
     }*/
    /*public async Task<HorizontalCoordinates> getAstroTracking_HorizontalCoordinates(Guid pGuid)
    {
        HorizontalCoordinates resault = null;
        using (var context = new AstroDbContext())
        {
            int contador = 0;

            while (contador < 5)
            {
                AstroTracking oAstroTracking = context.AstroTrackings.Where(x => x.publicID == pGuid && x.estado == 2).FirstOrDefault();
                if (oAstroTracking != null)
                {
                    resault = new HorizontalCoordinates() { Altitude = oAstroTracking.Altitude.Value, Azimuth = oAstroTracking.Azimuth.Value };
                    break;
                }
                await Task.Delay(500);
                contador++;
            }
        }
        return resault;
    }*/
    /* public bool changeAstroTrackingEstado(Guid pGuid, int pEstado)
     {
         bool result = false;
         // Crear e inicializar el contexto
         using (var context = new AstroDbContext())
         {
             // Buscar el usuario por ID
             AstroTracking oAstroTracking = context.AstroTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();


             if (oAstroTracking != null)
             {
                 // Actualizar los valores
                 oAstroTracking.estado = pEstado;

                 // Guardar los cambios en la base de datos
                 context.SaveChanges();
                 result = true;
             }
         }
         return result;
     }*/
    /* public bool removeAstroTrackingEstado(Guid pGuid)
     {
         bool result = false;
         // Crear e inicializar el contexto
         using (var context = new AstroDbContext())
         {
             // Buscar el usuario por ID
             AstroTracking oAstroTracking = context.AstroTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();


             if (oAstroTracking != null)
             {
                 context.AstroTrackings.Remove(oAstroTracking);
                 // Guardar los cambios en la base de datos
                 context.SaveChanges();
                 result = true;
             }
         }
         return result;
     }*/
    public string actionAnt(DateTime pDate, EquatorialCoordinates eq, bool isNew = false, bool isLaserOn = false)
    {
        string result = string.Empty;

        double siderealTime_local = AstronomyEngine.GetTSL(pDate, city);

        // var fff = _processCoordinatesStar.Start(eq.ra, eq.dec);
        // Guid oAstroTracking = saveAstroTracking(eq.ra, eq.dec);


        HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
        //HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(oAstroTracking).Result;
        //removeAstroTrackingEstado(oAstroTracking);
        if (hc != null)
        {
            ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
            if (oServoCoordinates != null)
            {
                HorariasCoordinates oHorariasCoordinates = AstronomyEngine.ToHorariasCoordinates(siderealTime_local, eq);
                string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(eq.ra) + "/" + AstronomyEngine.GetSexagesimal(eq.dec);
                string strHC = "HA/Dec: " + AstronomyEngine.GetHHmmss(oHorariasCoordinates.HA) + "/" + AstronomyEngine.GetSexagesimal(oHorariasCoordinates.dec);
                string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(hc.Altitude);
                result += strEq + "<br/>";
                //result += strHC + "<br/>";
                result += strHc + "<br/>";
                result += "HD " + eq.idHD.ToString() + "<br/>";
                string result_accion = "Servo: " + (isNew ? moveTheAnt_rango(oServoCoordinates, isLaserOn) : moveTheAnt(oServoCoordinates));
            }
            else
            {
                result = "Estrella no es visible";
            }
        }

        return result;
    }
    public string actionAnt_servo(double pHorizontal, double pVertical, double pH_min, double pH_max, double pV_min, double pV_max, bool pOnLaser)
    {
        string result = string.Empty;

        ServoCoordinates oServoCoordinates = new ServoCoordinates() { servoH = Math.Round(pHorizontal, 6), servoV = Math.Round(pVertical, 6) };//ServoCoordinates.convertServoCoordinates(hc);
        string strHc = "Horizontal/Vertical: " + AstronomyEngine.GetSexagesimal(pHorizontal) + "/" + AstronomyEngine.GetSexagesimal(pVertical);
        result += "Servo: " + moveTheAnt_rango(oServoCoordinates.servoH, oServoCoordinates.servoV, Math.Round(pH_min, 6), Math.Round(pH_max, 6), Math.Round(pV_min, 6), Math.Round(pV_max, 6), pOnLaser ? 1 : 0);
        return result;
    }
    public string moveTheAnt(ServoCoordinates pServoCoordinates)
    {
        return _processServo.Start(pServoCoordinates.servoH, pServoCoordinates.servoV, 1);
    }
    public string moveTheAnt(double pH, double pV, int pLaser)
    {
        //var ddd = Util.getLogs();
        //Util.log(new Exception(DateTime.Now.Millisecond.ToString()));
        //Util.log_file(new Log(new Exception(DateTime.Now.Millisecond.ToString())));
        return _processServo.Start(pH, pV, pLaser);
    }
    public string moveTheAnt_rango(ServoCoordinates pServoCoordinates, bool isLaserOn = false)
    {
        return moveTheAnt_rango(pServoCoordinates.servoH, pServoCoordinates.servoV, _Horizontal_grados_min, _Horizontal_grados_max, _Vertical_grados_min, _Vertical_grados_max, isLaserOn ? 1 : 0);
    }
    public string moveTheAnt_rango(double pH, double pV, double pH_min, double pH_max, double pV_min, double pV_max, int pLaser)
    {
        double horizontal = 0;
        double vertical = 0;
        if (_Horizontal_grados > pH)
        {
            horizontal = Math.Abs(_Horizontal_grados - pH);
        }
        else
        {
            horizontal = Math.Abs(pH - _Horizontal_grados);
        }
        if (_Vertical_grados > pV)
        {
            vertical = Math.Abs(_Vertical_grados - pV);
        }
        else
        {
            vertical = Math.Abs(pV - _Vertical_grados);
        }
        _Horizontal_grados = pH;
        _Vertical_grados = pV;
        _Horizontal_grados_min = pH_min;
        _Horizontal_grados_max = pH_max;
        _Vertical_grados_min = pV_min;
        _Vertical_grados_max = pV_max;
        double sleep_secs = 3;
        if (horizontal > vertical)
        {
            sleep_secs = double.Round((sleep_secs * horizontal) / 180.0, 1);
        }
        else
        {
            sleep_secs = double.Round((sleep_secs * vertical) / 180.0, 1);
        }
        if (sleep_secs < 0.5)
        {
            sleep_secs = 0.5;
        }

        return _processServoRango.Start(pH, pV, pH_min, pH_max, pV_min, pV_max, pLaser, sleep_secs);
    }
    public string actionLaser(int pIsRead, int pLaser)
    {
        return _processLaser.Start(pIsRead, pLaser);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _processServo.Dispose();
                _processLaser.Dispose();
                _processServoRango.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class ProcessServo : IDisposable
{
    private bool disposedValue = false;
    private PoolProcess _PoolProcess;
    public ProcessServo()
    {
        string nameFile = string.Empty;
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFile = "py_astro";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFile = "py_astro.exe";
        }
        _PoolProcess = new PoolProcess(1, nameFile);
    }

    public string Start(double pH, double pV, int pLaser)
    {
        string parameter = pH.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + pV.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + Convert.ToString(pLaser);
        return _PoolProcess.Start(parameter);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _PoolProcess.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class ProcessServoRango : IDisposable
{
    private bool disposedValue = false;
    private PoolProcess _PoolProcess;
    public ProcessServoRango()
    {
        string nameFile = string.Empty;
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFile = "py_astro_servos";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFile = "py_astro_servos.exe";
        }
        _PoolProcess = new PoolProcess(1, nameFile);
    }

    public string Start(double pH, double pV, double pH_min, double pH_max, double pV_min, double pV_max, int pLaser, double pSleep_secs)
    {
        string parameter = pH.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + pV.ToString(System.Globalization.CultureInfo.InvariantCulture) + " "
        + Convert.ToString(pLaser) + " " + pH_min.ToString(System.Globalization.CultureInfo.InvariantCulture) + " "
         + pH_max.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + pV_min.ToString(System.Globalization.CultureInfo.InvariantCulture) + " "
         + pV_max.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + pSleep_secs.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return _PoolProcess.Start(parameter);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _PoolProcess.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class ProcessCoordinatesStar : IDisposable
{
    private bool disposedValue = false;
    private PoolProcess _PoolProcess;
    public ProcessCoordinatesStar()
    {
        string nameFile = string.Empty;
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFile = "py_coordinatesStar";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFile = "python";// py_coordinatesStar.py "py_coordinatesStar.exe";
        }
        _PoolProcess = new PoolProcess(1, nameFile);
    }

    public string Start(double pRa, double pDec)
    {
        string parameter = "py_coordinatesStar.py " + pRa.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + pDec.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return _PoolProcess.Start(parameter);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _PoolProcess.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class ProcessLaser : IDisposable
{
    private bool disposedValue = false;
    private PoolProcess _PoolProcess;
    public ProcessLaser()
    {
        string nameFile = string.Empty;
        // 
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFile = "py_laser";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFile = "py_laser.exe";
        }
        _PoolProcess = new PoolProcess(1, nameFile);
    }

    public string Start(int pIsRead, int pLaser)
    {
        string parameter = Convert.ToString(pIsRead) + " " + Convert.ToString(pLaser);
        return _PoolProcess.Start(parameter);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _PoolProcess.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class PoolProcess : IDisposable
{
    private bool disposedValue = false;
    private readonly Queue<Process> recursosDisponibles;
    private readonly int maxRecursos;
    public PoolProcess(int pMaxRecursos, string pNameFile)
    {
        this.maxRecursos = pMaxRecursos;
        recursosDisponibles = new Queue<Process>();
        var pathAndFile = Path.Combine(nscore.Helper.folder, pNameFile);
        if (File.Exists(pathAndFile))
        {
            // Inicializar el pool con recursos preinstanciados
            for (int i = 0; i < maxRecursos; i++)
            {
                Process oProcess = new Process();
                var processInfo = new ProcessStartInfo
                {
                    FileName = pathAndFile,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                oProcess.StartInfo = processInfo;
                recursosDisponibles.Enqueue(oProcess);
            }
        }
    }
    public string Start(string pParameter)
    {
        string output = "null";
        try
        {
            Process oProcess = GetResource();
            if (oProcess != null)
            {
                oProcess.StartInfo.Arguments = pParameter;
                oProcess.Start();
                output = oProcess.StandardOutput.ReadToEnd();
                oProcess.WaitForExit();
                SetResource(oProcess);
            }
            else
            {
                output = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return output;
    }
    public Process GetResource()
    {
        if (recursosDisponibles.Count > 0)
        {
            return recursosDisponibles.Dequeue();
        }

        // Aquí puedes implementar la lógica para manejar el caso en el que no haya recursos disponibles,
        // como lanzar una excepción o esperar hasta que haya un recurso disponible.

        return null;
    }

    public void SetResource(Process recurso)
    {
        if (recursosDisponibles.Count < maxRecursos)
        {
            recursosDisponibles.Enqueue(recurso);
        }
        else
        {
            // Aquí puedes implementar la lógica para manejar el caso en el que el pool está lleno
            // y no se puede devolver el recurso.
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                //
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}