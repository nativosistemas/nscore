using System.Diagnostics;

namespace nscore;
public class ProcessAnt : IDisposable
{
    private bool disposedValue = false;
    //private Process _controller = new Process();
    private PoolProcessServo _poolProcessServo = new PoolProcessServo(1);
    private PoolProcessLaser _poolProcessLaser = new PoolProcessLaser(1);
    //private Process _controllerLaser = new Process();
    private List<Star> _l_Star = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
    //private static Semaphore _semaphore_ant = new Semaphore(0, 1);
    public ObserverCoordinates city { get { return _city; } set { _city = value; } }


    public ProcessAnt()
    {
        _l_Star = nscore.Util.getStars();
    }
    public List<Star> getStars()
    {
        foreach (Star oStar in _l_Star)
        {
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(city, eq);
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
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(city, eq);
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
        return moveTheAnt(pServoCoordinates.servoH, pServoCoordinates.servoV, 1);
    }
    public string moveTheAnt(double pH, double pV, int pLaser)
    {
        string output = "null";
        try
        {
            ProcessServo oProcessServo = _poolProcessServo.GetResource();
            if (oProcessServo != null)
            {
                oProcessServo.Start(pH, pV, pLaser);
                _poolProcessServo.SetResource(oProcessServo);
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
    public string actionLaser(int pIsRead, int pLaser)
    {
        string output = "null";
        try
        {
            ProcessLaser oProcess = _poolProcessLaser.GetResource();
            if (oProcess != null)
            {
                oProcess.Start(pIsRead, pLaser);
                _poolProcessLaser.SetResource(oProcess);
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
                _poolProcessServo.Dispose();
                _poolProcessLaser.Dispose();
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
    private Process _controller = new Process();
    public ProcessServo()
    {

        string nameFile = string.Empty;
        // 
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFile = "py_astro";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFile = "py_astro.exe";
        }
        var pathAndFile = Path.Combine(nscore.Helper.folder, nameFile);
        if (File.Exists(pathAndFile))
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = pathAndFile,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _controller.StartInfo = processInfo;
        }
    }
    public string Start(double pH, double pV, int pLaser)
    {
        string output = "null";
        try
        {
            double H = pH;
            double V = pV;
            int laser = pLaser;
            string parameter = H.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + V.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + Convert.ToString(laser);

            _controller.StartInfo.Arguments = parameter;
            _controller.Start();
            output = _controller.StandardOutput.ReadToEnd();
            _controller.WaitForExit();

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
                _controller.Dispose();
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
    private Process _controller = new Process();
    public ProcessLaser()
    {
        string nameFile = string.Empty;
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFile = "py_laser";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFile = "py_laser.exe";
        }
        var pathAndFile = Path.Combine(nscore.Helper.folder, nameFile);
        if (File.Exists(pathAndFile))
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = pathAndFile,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _controller.StartInfo = processInfo;
        }
    }
    public string Start(int pIsRead, int pLaser)
    {
        string output = "null";
        try
        {
            string parameter = Convert.ToString(pIsRead) + " " + Convert.ToString(pLaser);
            _controller.StartInfo.Arguments = parameter;
            _controller.Start();
            output = _controller.StandardOutput.ReadToEnd();
            _controller.WaitForExit();

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
                _controller.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class PoolProcessServo : IDisposable
{
    private bool disposedValue = false;
    private readonly Queue<ProcessServo> recursosDisponibles;
    private readonly int maxRecursos;

    public PoolProcessServo(int maxRecursos)
    {
        this.maxRecursos = maxRecursos;
        recursosDisponibles = new Queue<ProcessServo>();

        // Inicializar el pool con recursos preinstanciados
        for (int i = 0; i < maxRecursos; i++)
        {
            recursosDisponibles.Enqueue(new ProcessServo());
        }
    }

    public ProcessServo GetResource()
    {
        if (recursosDisponibles.Count > 0)
        {
            return recursosDisponibles.Dequeue();
        }

        // Aquí puedes implementar la lógica para manejar el caso en el que no haya recursos disponibles,
        // como lanzar una excepción o esperar hasta que haya un recurso disponible.

        return null;
    }

    public void SetResource(ProcessServo recurso)
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

            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class PoolProcessLaser : IDisposable
{
    private bool disposedValue = false;
    private readonly Queue<ProcessLaser> recursosDisponibles;
    private readonly int maxRecursos;

    public PoolProcessLaser(int maxRecursos)
    {
        this.maxRecursos = maxRecursos;
        recursosDisponibles = new Queue<ProcessLaser>();

        // Inicializar el pool con recursos preinstanciados
        for (int i = 0; i < maxRecursos; i++)
        {
            recursosDisponibles.Enqueue(new ProcessLaser());
        }
    }

    public ProcessLaser GetResource()
    {
        if (recursosDisponibles.Count > 0)
        {
            return recursosDisponibles.Dequeue();
        }

        // Aquí puedes implementar la lógica para manejar el caso en el que no haya recursos disponibles,
        // como lanzar una excepción o esperar hasta que haya un recurso disponible.

        return null;
    }

    public void SetResource(ProcessLaser recurso)
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

            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}