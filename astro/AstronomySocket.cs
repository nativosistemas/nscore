using System.Net.Sockets;
using System.Net;
using System.Text;

namespace nscore;
public class AstronomySocket : IDisposable
{
    private bool disposedValue = false;
    public AstronomySocket()
    {

    }

    public static void enviarInfo(int pId)
    {
        HorizontalCoordinates hc = getCoordenadas(pId);
        if (hc != null)
        {
            sendData(hc);
        }
    }
    public static HorizontalCoordinates getCoordenadas(int pId)
    {
        HorizontalCoordinates result = null;
        EquatorialCoordinates eq = null;
        ObserverCoordinates ciudad = ObserverCoordinates.cityRosario;
        switch (pId)
        {
            case 1:
                eq = new EquatorialCoordinates() { dec = -16.7280, ra = 101.28326 };//Sirio
                break;
            case 2:
                eq = new EquatorialCoordinates() { dec = 19.1685, ra = 214.18623 };//Sirio
                break;
            default:
                break;
        }
        if (eq != null)
        {
            result = AstronomyEngine.ToHorizontalCoordinates(ciudad, eq);
        }
        return result;
    }
    public static void sendData(HorizontalCoordinates pHorizontalCoordinates)
    {
        try
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            int nro_puerto = 10000;
            // Configurar la dirección IP y el puerto local para recibir datos
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), nro_puerto);
       
               ServoCoordinates oServoCoordinates =      ServoCoordinates.convertServoCoordinates(pHorizontalCoordinates);


            string responseData = Convert.ToInt32(oServoCoordinates.servoH).ToString() + "_" + Convert.ToInt32(oServoCoordinates.servoV).ToString() + "_0";// "Hola desde el servidor";
            byte[] responseBytes = Encoding.Unicode.GetBytes(responseData);
            // Envía los datos
            socket.SendTo(responseBytes, localEndPoint);

            // Cerrar el socket cuando hayas terminado de usarlo
            socket.Close();
        }
        catch (Exception ex)
        {
            DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
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