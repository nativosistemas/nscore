using System.Device.Gpio;

namespace nscore;
public class ServoController
{
    private readonly GpioController gpioController;
    // private readonly PwmChannel _pwmChannel;
    public int servoPin_18 = 18;
    //   private const double MinAngle = 0.0;
    //  private const double MaxAngle = 1.0;

    private const double minValue = 1000; // microsegundos
    private const double maxValue = 2000; // microsegundos
                                          // public double position = 0.0; //position (en grados)
    public double frequency_Hz = 50; // 50 Hz =>  unidad de frecuencia: El periodo cada segundo 
    public double getPeriod { get { return 1.0 / frequency_Hz; } }
    private double _lastPositionDegree;

    public ServoController()
    {
        gpioController = new GpioController(PinNumberingScheme.Board);
        //  int servoPin = 18;
        gpioController.OpenPin(servoPin_18, PinMode.Output);
    }

    public void SetServoPosition(double pPositionDegree)
    {
        _lastPositionDegree = pPositionDegree;
        double dutyCycle = pPositionDegree / 180.0 * (maxValue - maxValue) + minValue;
        int pulseWidth = (int)(dutyCycle / getPeriod * 1000000); // PWM

        //  gpioController.SetPwmFrequency(servoPin, frequency_Hz);
        //   gpioController.SetPwmDutyCycle(servoPin, pulseWidth);
    }
    public void logica()
    {

        try
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), "hola mundo", DateTime.Now);
            bool ledOn = true;
            while (true)
            {
                /* if (gpioController.Read(servoPin_18) == PinValue.High)
                 {
                     gpioController.Write(servoPin_18, PinValue.Low);
                 }
                 else
                 {
                     gpioController.Write(servoPin_18, PinValue.High);
                 }*/
                gpioController.Write(servoPin_18, ((ledOn) ? PinValue.High : PinValue.Low));
                Thread.Sleep(1000);
                ledOn = !ledOn;
            }

        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
            gpioController.ClosePin(servoPin_18);
        }

    }
    public double GetPositionDegree()
    {
        return _lastPositionDegree;
    }
}
