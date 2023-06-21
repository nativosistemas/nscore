using System.Diagnostics;

namespace nscore;
public class ProcessAnt : IDisposable
{
    private bool disposedValue = false;
    private Process _controller = new Process();
    public ProcessAnt()
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

        var pathAndFile = Path.Combine(nscore.Util.WebRootPath, @"files", nameFile);
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
            _controller.Start();

            output = _controller.StandardOutput.ReadToEnd();

            _controller.WaitForExit();
        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
            System.Console.WriteLine(ex);
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