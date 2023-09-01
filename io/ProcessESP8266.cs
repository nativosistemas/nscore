
using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace nscore;
public class ProcessESP8266 : IDisposable
{
    private bool disposedValue = false;
    
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // _PoolProcess.Dispose();
            }

            disposedValue = true;
        }
    }
    public static void saveDHT(DHT pDHT)
    {
        using (var context = new IoTDbContext())
        {
            context.DHT.Add(pDHT);
            context.SaveChanges();
        }
    }
    public static List<DHT> getDHT()
    {
        using (var context = new IoTDbContext())
        {
           return context.DHT.ToList();          
        }
    }
    public void Dispose()
    {
        Dispose(true);
    }
}