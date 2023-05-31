using System.Device.Gpio;
using System.Net.Sockets;
using System.Net;
using System.Text;

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
       // IPEndPoint ipEndPoint = new(ipAddress, 11_000);
        // Crear un nuevo socket UDP
        Socket socket = new Socket(AddressFamily.InterNetwork , SocketType.Dgram, ProtocolType.Udp);
        int nro_puerto = 10000;
        // Configurar la dirección IP y el puerto local para recibir datos
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), nro_puerto);

        // Vincular el socket a la dirección local
        socket.Bind(localEndPoint);

        // Buffer para almacenar los datos recibidos
        byte[] buffer = new byte[1024];

        // Recibir datos en el socket
        int bytesRead = socket.Receive(buffer);

        // Convertir los datos recibidos en una cadena
        string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Datos recibidos: " + receivedData);

        // Enviar datos al remitente

        ObserverCoordinates ciudad = ObserverCoordinates.cityRosario;
        EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = -16.7280, ra = 101.28326 };
          HorizontalCoordinates sirio_horizontal = AstronomyEngine.ToHorizontalCoordinates(ciudad, sirio_eq);
        string responseData = Convert.ToInt32( sirio_horizontal.Altitude).ToString() + "_"+ Convert.ToInt32( sirio_horizontal.Azimuth).ToString() + "_0";// "Hola desde el servidor";
        byte[] responseBytes = Encoding.ASCII.GetBytes(responseData);
        socket.SendTo(responseBytes, bytesRead, SocketFlags.None, localEndPoint);

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