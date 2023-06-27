using System.Net.Sockets;
using System.Net;
using System.Text;
using Python.Runtime;

namespace nscore;
public class AstronomyPython : IDisposable
{
    private bool disposedValue = false;
    public AstronomyPython()
    {

    }


    public static string sendStar(int pId)
    {
        string result = string.Empty;
        ObserverCoordinates city = ObserverCoordinates.cityRosario;
        Star oStar = nscore.Util.getStars().Where(x => x.nameBayer == pId).FirstOrDefault();
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
                    result = sendStarToServo(oServoCoordinates);
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
    public static string sendStarToServo(ServoCoordinates pServoCoordinates)
    {
        string result = string.Empty;
        try
        {
            string envPythonHome = @"C:\Python39\python39.dll";
            ///usr/bin/python2.7
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {///

                envPythonHome = @"/usr/bin/python";///python2.7
            }
            // your PATH environment variable as well.
            Runtime.PythonDLL = envPythonHome;//"python310.dll";

            PythonEngine.Initialize();

            using (Py.GIL()) // Inicializa el Global Interpreter Lock (GIL) de Python
            {
                var nameFile = Path.Combine(nscore.Util.WebRootPath, @"files", "py_servo.py");
                if (File.Exists(nameFile))
                {
                    dynamic py = Py.Import("__main__"); // Importa el módulo principal de Python
                    py.exec(File.ReadAllText(nameFile)); // Ejecuta el script Python "py_servo.py"

                    dynamic moveServo = py.moveServo; // Obtiene la referencia a la función Python

                    moveServo = moveServo(Convert.ToInt32(pServoCoordinates.servoH), Convert.ToInt32(pServoCoordinates.servoV), 1); // Llama a la función Python

                    Console.WriteLine(result); // Imprime el resultado
                }
            }

            // string responseData = Convert.ToInt32(pServoCoordinates.servoH).ToString() + "_" + Convert.ToInt32(pServoCoordinates.servoV).ToString() + "_0";// "Hola desde el servidor";

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
        return result;
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