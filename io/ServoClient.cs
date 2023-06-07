using System.Device.Gpio;
using System.Net.Sockets;
using System.Net;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace nscore;
public class ServoClient : IDisposable
{
    private const int LedPin = 24;
    private const int servo1Pin = 17; // Cambiar al número de pin GPIO correspondiente
    private const int servo2Pin = 27; // Cambiar al número de pin GPIO correspondiente
    private GpioController _controller = new GpioController();
    private bool disposedValue = false;

    public ServoClient()
    {
        _controller.OpenPin(LedPin, PinMode.Output);
        _controller.Write(LedPin, PinValue.Low);

        _controller.OpenPin(servo1Pin, PinMode.Output);
        _controller.OpenPin(servo2Pin, PinMode.Output);
        _controller.Write(servo1Pin, PinValue.Low);
        _controller.Write(servo2Pin, PinValue.Low);

    }
    public void moveStar()
    {
        MoverServo(servo1Pin, 90); // Angulo en grados para servo1
        MoverServo(servo2Pin, 45); // Angulo en grados para servo2
        LedOn();
    }
    public void Main_Socket()
    {

        // Crear un nuevo socket UDP
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        int nro_puerto = 10000;
        // Configurar la dirección IP y el puerto local para recibir datos
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), nro_puerto);


        // Enviar datos al remitente

        ObserverCoordinates ciudad = ObserverCoordinates.cityRosario;
        EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = -16.7280, ra = 101.28326 };
        HorizontalCoordinates sirio_horizontal = AstronomyEngine.ToHorizontalCoordinates(ciudad, sirio_eq);
        string responseData = Convert.ToInt32(sirio_horizontal.Altitude).ToString() + "_" + Convert.ToInt32(sirio_horizontal.Azimuth).ToString() + "_0";// "Hola desde el servidor";
        byte[] responseBytes = Encoding.Unicode.GetBytes(responseData);
        socket.SendTo(responseBytes, localEndPoint);

        // Cerrar el socket cuando hayas terminado de usarlo
        socket.Close();
    }
    public void sendData(HorizontalCoordinates pHorizontalCoordinates)
    {

        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        int nro_puerto = 10000;
        // Configurar la dirección IP y el puerto local para recibir datos
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), nro_puerto);

        string responseData = Convert.ToInt32(pHorizontalCoordinates.Altitude).ToString() + "_" + Convert.ToInt32(pHorizontalCoordinates.Azimuth).ToString() + "_0";// "Hola desde el servidor";
        byte[] responseBytes = Encoding.Unicode.GetBytes(responseData);
        // Envía los datos
        socket.SendTo(responseBytes, localEndPoint);

        // Cerrar el socket cuando hayas terminado de usarlo
        socket.Close();
    }
    public void MoverServo(int pin, double angulo)
    {

        // Calcular el valor del pulso en microsegundos para el ángulo deseado
        // 0.5 ms (ángulo 0) + ((2.4 ms - 0.5 ms) / 180) * angulo
        var pulso = 0.5 + ((2.4 - 0.5) / 180.0) * angulo;

        // Convertir el pulso a tiempo en ticks (100 ns)
        var tiempo = (int)(pulso * 10000.0);

        // Generar el pulso en el pin GPIO durante el tiempo calculado
        _controller.Write(pin, PinValue.High);
        Thread.Sleep(tiempo);
        _controller.Write(pin, PinValue.Low);
    }
    public void LedOn()
    {
        _controller.Write(LedPin, PinValue.High);
    }

    public void LedOff()
    {
        _controller.Write(LedPin, PinValue.Low);
    }
    public static string MainPython()
    {
        string result = "(vacio)";
        var nameFile = Path.Combine(nscore.Util.WebRootPath, @"files", "py_serverV3.py");
        if (File.Exists(nameFile))
        {
            // Ruta al intérprete de Python
            string pythonInterpreter = "python";

            // Ruta al archivo Python que contiene la función
            string pythonFile = nameFile;//"ruta/al/archivo.py";

            // Nombre de la función que deseas ejecutar
            string functionName = "helloWord";

            // Argumentos para pasar a la función (opcional)
            string arguments = "fff";

            // Crear el proceso externo para ejecutar Python
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = pythonInterpreter;
            startInfo.Arguments = $"{pythonFile} {functionName} {arguments}";
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            // Iniciar el proceso
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo = startInfo;
            process.Start();

            // Leer la salida de la función de Python
            string output = process.StandardOutput.ReadToEnd();

            // Esperar a que el proceso termine
            process.WaitForExit();
            result = output;
        }
        // Imprimir la salida de la función
        //  Console.WriteLine(output);
        return result;
    }
    public static string MainPython_v2()
    {
        string result = "(vacio)";
        var nameFile = Path.Combine(nscore.Util.WebRootPath, @"files", "py_serverV3.py");
        if (File.Exists(nameFile))
        {

            ScriptRuntime py = Python.CreateRuntime();
            dynamic pyProgram = py.UseFile(nameFile);
            double h = 90.1;
            double v = 45.1;
            result = pyProgram.moveServo(h, v);



        }

        return result;
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _controller.Write(LedPin, PinValue.Low);
                _controller.Write(servo1Pin, PinValue.Low);
                _controller.Write(servo2Pin, PinValue.Low);
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