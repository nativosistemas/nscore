using System.Diagnostics;

namespace nscore;
public class ProcessAnt : IDisposable
{
    private bool disposedValue = false;
    private Process _controller = new Process();
    private Process _controllerLaser = new Process();
    private List<Star> _l_Star = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
    public ObserverCoordinates city { get { return _city; } set { _city = value; } }


    public ProcessAnt()
    {
        _l_Star = nscore.Util.getStars();
        string nameFileLaser = string.Empty;
        string nameFileServo = string.Empty;
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFileServo = "py_astro";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFileServo = "py_astro.exe";
        }
        var pathAndFile = Path.Combine(DKbase.Helper.getFolder, @"files", nameFileServo);//nscore.Util.WebRootPath
        if (File.Exists(pathAndFile))
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = pathAndFile,
                //Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _controller.StartInfo = processInfo;
        }
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            nameFileLaser = "py_laser";
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            nameFileLaser = "py_laser.exe";
        }

        var pathAndFileLaser = Path.Combine(DKbase.Helper.getFolder, @"files", nameFileLaser);
        if (File.Exists(pathAndFileLaser))
        {
            var processInfoLaser = new ProcessStartInfo
            {
                FileName = pathAndFileLaser,
                //Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _controllerLaser.StartInfo = processInfoLaser;
        }
        else
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), "no find File.Exists(pathAndFile)", DateTime.Now);
        }

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
                    result += moveTheAnt(oServoCoordinates);
                    actionLaser(0, 1);
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
            double H = pH;
            double V = pV;
            int laser = pLaser;
            string parameter = H.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + V.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + Convert.ToString(laser);
            _controller.StartInfo.Arguments = parameter;
            //_controller.StartInfo.FileName = nameFileServo;
            _controller.Start();

            output = _controller.StandardOutput.ReadToEnd();

            _controller.WaitForExit();
        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
            //System.Console.WriteLine(ex);
        }
        return output;
    }
    public string actionLaser(int pIsRead, int pLaser)
    {
        string output = "null";
        try
        {
            int laser = pLaser;
            string parameter = Convert.ToString(pIsRead) + " " + Convert.ToString(laser);
            _controllerLaser.StartInfo.Arguments = parameter;
            _controllerLaser.Start();

            output = _controllerLaser.StandardOutput.ReadToEnd();

            _controllerLaser.WaitForExit();
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