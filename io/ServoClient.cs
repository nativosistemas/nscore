using System.Device.Gpio;
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