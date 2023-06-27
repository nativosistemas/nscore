using System.Diagnostics;

namespace nscore;
public class ProcessAnt : IDisposable
{
    private bool disposedValue = false;
    //private Process _controller = new Process();
    private ProcessServo _processServo = new ProcessServo();
    private ProcessLaser _processLaser = new ProcessLaser();
    //private Process _controllerLaser = new Process();
    private List<Star> _l_Star = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
    //private static Semaphore _semaphore_ant = new Semaphore(0, 1);
    public ObserverCoordinates city { get { return _city; } set { _city = value; } }


    public ProcessAnt()
    {
        _l_Star = nscore.AstroDbContext.getStars();
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
        //ObserverCoordinates city = ObserverCoordinates.cityRosario;
        Star oStar = _l_Star.Where(x => x.nameBayer == pId).FirstOrDefault();
        if (oStar != null)
        {
            double siderealTime_local = AstronomyEngine.GetTSL(city);
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
            if (hc != null)
            {
                ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
                if (oServoCoordinates != null)
                {
                    string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(eq.ra) + "/" + AstronomyEngine.GetSexagesimal(eq.dec);
                    string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(hc.Altitude);
                    result += strEq + "\n" + strHc + "\n";
                    //_semaphore_ant.WaitOne(); // Intentar adquirir un recurso del semáforo
                    result += moveTheAnt(oServoCoordinates);
                    actionLaser(0, 1);
                    // _semaphore_ant.Release(); // Liberar el recurso en el semáforo
                }
                else
                {
                    result = "Estrella no es visible";
                }
            }
        }
        else
        {
            result = "No se encontro estrella";
        }
        return result;
    }
    public string moveTheAnt(ServoCoordinates pServoCoordinates)
    {
        return _processServo.Start(pServoCoordinates.servoH, pServoCoordinates.servoV, 1);
    }
    public string moveTheAnt(double pH, double pV, int pLaser)
    {
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
        string output = "null";
        try
        {
            Process oProcess = _PoolProcess.GetResource();
            if (oProcess != null)
            {
                double H = pH;
                double V = pV;
                int laser = pLaser;
                string parameter = H.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + V.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + Convert.ToString(laser);
                oProcess.StartInfo.Arguments = parameter;
                oProcess.Start();
                output = oProcess.StandardOutput.ReadToEnd();
                oProcess.WaitForExit();
                _PoolProcess.SetResource(oProcess);
            }
            else
            {
                output = "recurso en uso";
            }
        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
        return output;
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
        string output = "null";
        try
        {
            Process oProcess = _PoolProcess.GetResource();
            if (oProcess != null)
            {
                string parameter = Convert.ToString(pIsRead) + " " + Convert.ToString(pLaser);
                oProcess.StartInfo.Arguments = parameter;
                oProcess.Start();
                output = oProcess.StandardOutput.ReadToEnd();
                oProcess.WaitForExit();
                _PoolProcess.SetResource(oProcess);
            }
            else
            {
                output = "recurso en uso";
            }
        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
        return output;
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