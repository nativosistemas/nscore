using System.Net.Sockets;
using System.Net;
using System.Text;

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
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(city, eq);
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