using System.Device.Gpio;
namespace nscore;
public class LedClient : IDisposable
{
    private const int LedPin = 24;
 
    private GpioController _controller = new GpioController();
    private bool disposedValue = false;
 
    public LedClient()
    {
        _controller.OpenPin(LedPin, PinMode.Output);
        _controller.Write(LedPin, PinValue.Low);
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