using System;
using System.IO;

namespace nscore;

public class ProcessFile : IDisposable
{
    private bool disposedValue = false;

    public static void ffff()
    {

        HorariasCoordinates dept = new HorariasCoordinates() { dec = 101, HA = 33 };
        string strJson = Serializador.SerializarAJson(dept);
        //System.Text.Json.JsonSerializer.Serialize(dept);

        byte[] byteArray_strJson = System.Text.Encoding.UTF8.GetBytes(strJson);

        HorariasCoordinates ex = System.Text.Json.JsonSerializer.Deserialize<HorariasCoordinates>(byteArray_strJson);
        string jsonData = "{\"_id\":\"test121\", " +
                     "\"username\":\"test123\", " +
                      "\"password\": \"test123\"}";
        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(jsonData);
        //  nscore.Helper.folder
        File.WriteAllBytes(@"C:\data.dmp", byteArray_strJson);
    }
    public static void saveFile(string pNameFile, byte[] pBuffer)
    {
        try
        {
            File.WriteAllBytes(Path.Combine(nscore.Helper.folder, @"files", pNameFile), pBuffer);
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
    }
    public static IEnumerable<string> ReadLines(string pPath)
    {
        IEnumerable<string> result = null;
        if (!string.IsNullOrWhiteSpace(pPath) && System.IO.File.Exists(pPath))
        {
            try
            {
                result = File.ReadLines(pPath);
            }
            catch (Exception ex)
            {
                Util.log(ex);
            }
        }
        return result;
    }
    public static string ReadAllText(string pPath)
    {
        string result = null;
        if (!string.IsNullOrWhiteSpace(pPath) && System.IO.File.Exists(pPath))
        {
            try
            {
                result = File.ReadAllText(pPath);
            }
            catch (Exception ex)
            {
                Util.log(ex);
            }
        }
        return result;
    }
    public static string GetStringAstronomy()
    {
        // List<int> l_column = new List<int> { 5 };
        string result = null;
        //
        string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", "simbadEstrellas.csv");
        //result = ReadAllText(pathAstronomy);
        IEnumerable<string> lines = File.ReadLines(pathAstronomy);
        foreach (string oLine in lines)
        {
            string resultLine = string.Empty;
            string[] words = oLine.Split(',');
            string[] names = words[4].Split('|');
            foreach (string oName in names)
            {
                if (("|" + oName).Contains("|HD "))
                {
                    resultLine = oName;
                    break;
                }
            }
            result += resultLine + Environment.NewLine;
        }

        //
        return result;
    }
    public static List<string> GetListStringAstronomy(string pPath)
    {
        List<string> result = new List<string>();
        string pathAstronomy = Path.Combine(nscore.Util.WebRootPath, @"files", pPath);
        IEnumerable<string> lines = File.ReadLines(pathAstronomy).ToList();
        result = lines.ToList();
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