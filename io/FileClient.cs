using System.Diagnostics;
namespace nscore;
public class FileClient : IDisposable
{

    private bool disposedValue = false;

    public FileClient()
    {

    }

    /*   public void Start(string pPathAndFile)
       {
           try
           {
               var p = new Process();
               p.StartInfo = new ProcessStartInfo(pPathAndFile)//@"file.exe"
               {
                   Verb = "runas",
                   UseShellExecute = true,
                   WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
               };

               p.Start();
               p.WaitForExit();
           }
           catch (Exception ex)
           {
               Console.WriteLine(ex.Message);
               DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
           }
       }*/
    public static string run()
    {
        string result = null;
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
            double H = 45.6;
            double V = 97.28;
            int laser = 1;
            //double decimalUSA = double.Parse( H , System.Globalization.CultureInfo.InvariantCulture);
            string parameter = H.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + V.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + Convert.ToString(laser);

            string output = RunProcessAndGetOutput(pathAndFile, parameter);//RunProcessAndGetOutput("dotnet", "--version");
            result = output;// ProcessStart(nameFile);
        }
        return result;
    }
    public static string ProcessStart(string pPathAndFile)
    {
        string result = null;
        //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location)
        ProcessStartInfo startInfo = null;
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
        {
            startInfo = new ProcessStartInfo()
            {
                FileName = pPathAndFile,//"<Full Path to the linux exe>"
                                        // Arguments = "<Arguments if any>",
                UseShellExecute = false, //Import in Linux environments
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }
        else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            startInfo = new ProcessStartInfo(pPathAndFile)
            {
                Verb = "runas",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
        }
        if (startInfo != null)
        {
            var process = new Process()
            {
                StartInfo = startInfo,

            };
            process.OutputDataReceived += (sender, data) =>
                    {
                        System.Console.WriteLine(data.Data);
                    };

            process.ErrorDataReceived += (sender, data) =>
            {
                DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), data.Data, DateTime.Now);
                System.Console.WriteLine(data.Data);
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                result = process.StandardOutput.ReadToEnd();
            }
            catch (Exception ex)
            {
                DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
                System.Console.WriteLine(ex);
            }
        }
        return result;
    }
    public static string RunProcessAndGetOutput(string fileName, string arguments)
    {
        string output = null;
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };



            using (var process = new Process())
            {
                process.StartInfo = processInfo;
                process.Start();

                output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();
            }
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

            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}