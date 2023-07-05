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
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            int nro_puerto = 10000;
            // Configurar la dirección IP y el puerto local para recibir datos
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), nro_puerto);

            string responseData = Convert.ToInt32(pServoCoordinates.servoH).ToString() + "_" + Convert.ToInt32(pServoCoordinates.servoV).ToString() + "_0";// "Hola desde el servidor";
            Console.WriteLine("responseData: " + responseData);
            result = responseData;
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseData);
            // Envía los datos
            socket.SendTo(responseBytes, localEndPoint);

            /*
           // Leer datos respuesta
           // Buffer para almacenar los datos leídos
           byte[] buffer = new byte[1024];
           // Leer datos del socket
           int bytesRead = socket.Receive(buffer);

           // Convertir los datos recibidos a una cadena de texto
           string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
           Console.WriteLine("dataReceived: " + dataReceived);
           */
            // Cerrar el socket cuando hayas terminado de usarlo
            socket.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Util.log(ex);
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