using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;


namespace nscore
{
    public static class Serializador
    {
        public static string SerializarAJson(this object objeto)
        {
            string jsonResultado = string.Empty;
            try
            {
                jsonResultado = JsonSerializer.Serialize(objeto);
            }
            catch (Exception ex) {
               // DKbase.generales.Log.LogError(MethodBase.GetCurrentMethod(), ex, DateTime.Now, objeto);
            }
            return jsonResultado;
        }
        public static T DeserializarJson<T>(this string jsonSerializado)
        {
            try
            {
                T obj  = JsonSerializer.Deserialize<T>(jsonSerializado);
                return obj;
            }
            catch (Exception ex)
            {
               // DKbase.generales.Log.LogError(MethodBase.GetCurrentMethod(), ex, DateTime.Now, jsonSerializado);
                return default(T);
            }
        }
    }
}
