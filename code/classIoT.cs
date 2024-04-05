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
[Index(nameof(publicID), IsUnique = true)]
public class User
{
    public User()
    {
        publicID = Guid.NewGuid();
        createTime = DateTime.Now;
    }
    [Key]
    public Guid publicID { get; set; }
    public DateTime createTime { get; set; }
    public string? info { get; set; }
    public string login { get; set; }
    public string pass { get; set; }
    public string name { get; set; }

}
[Index(nameof(publicID), IsUnique = true)]
public class SessionUser
{
    public SessionUser()
    {
        publicID = Guid.NewGuid();
        createTime = DateTime.Now;
       // user_publicID = pUser_publicID;
    }

    [Key]
    public Guid publicID { get; set; }
    public Guid user_publicID { get; set; }
    public DateTime createTime { get; set; }
    public string? info { get; set; }
}
public class request_User
{
    public string name { get; set; }
    public string pass { get; set; }
}