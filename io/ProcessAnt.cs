using System.Diagnostics;

namespace nscore;
public class ProcessAnt : IDisposable
{
    private bool disposedValue = false;
    //private Process _controller = new Process();
    private bool _versionNew = true;
    private ProcessServo _processServo = new ProcessServo();
    private ProcessLaser _processLaser = new ProcessLaser();
    //private Process _controllerLaser = new Process();
    private List<Star> _l_Star = null;
    private List<ObserverCoordinates> _l_City = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
    //private static Semaphore _semaphore_ant = new Semaphore(0, 1);
    public ObserverCoordinates city { get { return _city; } set { _city = value; } }


    public ProcessAnt()
    {
        if (_versionNew)
        {
            _city = getCitys().FirstOrDefault();
            _l_Star = new List<Star>();
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
                //oStar.nameBayer = oStar.id;
                _l_Star.Add(oStar);
            }
        }
        else
        {
            _l_Star = nscore.AstroDbContext.getStars();
        }
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
            _l_City.Add(new ObserverCoordinates() { id = 1, name = "Rosario", latitude = -32.9575, longitude = -60.639444, altitude = 0 });
            _l_City.Add(new ObserverCoordinates() { id = 2, name = "Quito", latitude = -0.22, longitude = -78.5125, altitude = 0 });
            _l_City.Add(new ObserverCoordinates() { id = 3, name = "Atenas", latitude = 37.984167, longitude = 23.728056, altitude = 0 });//,
        }
        return _l_City;
    }
    public List<Star> getStars()
    {
        double siderealTime_local = AstronomyEngine.GetTSL(city);
        foreach (Star oStar in _l_Star)
        {
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
            ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
            if (oServoCoordinates != null)
            {
                oStar.visible = true;
            }
            else
            {
                oStar.visible = false;
            }
        }
        return _l_Star;
    }
    public string findStar(int pId)
    {
        string result = string.Empty;
        Star oStar = _l_Star.Where(x => x.id == pId).FirstOrDefault();
        if (oStar != null)
        {
            result = findStar(oStar.id, oStar.dec,oStar.ra);
        }
        else
        {
            result = "No se encontro estrella";
        }
        return result;
    }
    public string findStar(int pIdHD, double pDec, double pRa)
    {
        string result = string.Empty;
        EquatorialCoordinates eq = new EquatorialCoordinates() { idHD = pIdHD, dec = pDec, ra = pRa };
        result = actionAnt(eq);
        return result;
    }
    public string actionAnt(EquatorialCoordinates eq)
    {
        string result = string.Empty;

        double siderealTime_local = AstronomyEngine.GetTSL(city);

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
                result += "HD " + eq.idHD.ToString() + "<br/>";
                result += "Servo: " + moveTheAnt(oServoCoordinates);
            }
            else
            {
                result = "Estrella no es visible";
            }
        }

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