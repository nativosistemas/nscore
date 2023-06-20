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
        var nameFile = Path.Combine(nscore.Util.WebRootPath, @"files", "py_test.exe");
        if (File.Exists(nameFile))
        {
            string output = RunProcessAndGetOutput(nameFile, "");//RunProcessAndGetOutput("dotnet", "--version");
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