using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace nscore;

public class IoT
{
     [Key]
    public Guid PublicID { get; set; }
    public string NameIoT { get; set; }
    public string chipID { get; set; }
    public DateTime fecha { get; set; }
}

public class DHT : IoT
{
    public double Humidity { get; set; }
    public double Temperature { get; set; }
}