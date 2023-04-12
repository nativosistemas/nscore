using System.Device.Gpio;

namespace nscore;
public interface IServoController
{
    public bool logica();
}
public class ServoController : IServoController
{
    private readonly GpioController gpioController;
    // private readonly PwmChannel _pwmChannel;
    public int servoPin_24_Gpio10 = 24;
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
        try
        {
            gpioController = new GpioController(PinNumberingScheme.Board);

        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
    }

    public void SetServoPosition(double pPositionDegree)
    {
        _lastPositionDegree = pPositionDegree;
        double dutyCycle = pPositionDegree / 180.0 * (maxValue - maxValue) + minValue;
        int pulseWidth = (int)(dutyCycle / getPeriod * 1000000); // PWM

        //  gpioController.SetPwmFrequency(servoPin, frequency_Hz);
        //   gpioController.SetPwmDutyCycle(servoPin, pulseWidth);
    }
 bool ledOn = false;
    public bool logica()
    {
       
        try
        {
            gpioController.OpenPin(servoPin_24_Gpio10, PinMode.Output);
            //DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), "hola mundo", DateTime.Now);

            // int contador = 0;
            //while (contador <= 90) // un minuto y medio 
            {
                if (ledOn)
                {
                    gpioController.Write(servoPin_24_Gpio10, PinValue.Low);
                    ledOn = false;
                }
                else
                {
                    gpioController.Write(servoPin_24_Gpio10, PinValue.High);
                    ledOn = true;
                }
                //gpioController.Write(servoPin_24_Gpio10, ((ledOn) ? PinValue.High : PinValue.Low));
                //Thread.Sleep(1000);
                ledOn = !ledOn;
            }

        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);

        }
        finally
        {
            try
            {
                gpioController.ClosePin(servoPin_24_Gpio10);
            }
            catch (Exception ex_finally)
            {
                DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex_finally, DateTime.Now);
            }
        }
        return ledOn;

    }
    public double GetPositionDegree()
    {
        return _lastPositionDegree;
    }
}
