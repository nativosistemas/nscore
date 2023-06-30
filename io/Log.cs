using System.ComponentModel.DataAnnotations;

namespace nscore;
public class Log
{
    public Log()
    { }
    public Log(Exception pEx)
    {
        app = Helper.app;
        fecha = DateTime.Now;
        Message = pEx.Message;
        InnerException = pEx.InnerException != null ? pEx.InnerException.ToString() : null;
        StackTrace = pEx.StackTrace;
        Message = pEx.Message;
    }
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public DateTime fecha { get; set; }
    public string app { get; set; }
    public string Message { get; set; }
    public string? InnerException { get; set; }
    public string? StackTrace { get; set; }
    public string? info { get; set; }


}