using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace nscore;
[Index(nameof(id), IsUnique = true)]
public class Constellation
{
    public Constellation()
    {
        publicID = Guid.NewGuid();
    }
    [Key]
    public Guid publicID { get; set; }
    [Required]
    public int id { get; set; }
    public int idHD_startRef { get; set; }
    public string? nameLatin { get; set; }
    public string? name { get; set; }
    public string? abbreviation { get; set; }
    public string? Genitivo { get; set; }
    public string? Origen { get; set; }
    public string? DescritaPor { get; set; }
    public double? Extension { get; set; }
    public double? ra { get; set; }
    public double? dec { get; set; }
    public bool visible { get; set; }

}
[Index(nameof(idHD), IsUnique = true)]
public class AstronomicalObject
{
    public AstronomicalObject()
    {
        publicID = Guid.NewGuid();
    }
    [Key]
    public Guid publicID { get; set; }
    [Required]
    public int idHD { get; set; }
    public string? nameLatin { get; set; }
    public double? magnitudAparente { get; set; }
    public string? bayerDesignation { get; set; }
    public string? name { get; set; }
    public double? ra { get; set; }
    public double? dec { get; set; }
    public string? simbadNames { get; set; }
    public string? simbadNameDefault { get; set; }
    public string? abbreviationConstellation { get; set; }
    public int? simbadOID { get; set; }
    public string getName()
    {
        string result = string.Empty;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(bayerDesignation))
        {
            result = name + " (" + bayerDesignation + ")";
        }
        else if (!string.IsNullOrEmpty(name))
        {
            result = name;
        }
        else if (!string.IsNullOrEmpty(nameLatin))
        {
            result = nameLatin;
        }
        else
        {
            result = "HD " + idHD.ToString();
        }

        return result;
    }
}
//[Keyless]
public class Star
{
    /* public Star(){
         guid = Guid.NewGuid();
     }
     [Key]
     public Guid guid { get; set; }*/
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public int nameBayer { get; set; }
    public string name { get; set; }
    public double ra { get; set; }
    public double dec { get; set; }
    public bool visible { get; set; }
      public int idHD { get; set; }
}
public class ObserverCoordinates
{
    public int id { get; set; }
    public string name { get; set; }
    // Coordenadas geográficas del observador
    public double latitude { get; set; }//= 19.4326; // en grados
    public double longitude { get; set; }//= -99.1332; // en grados
    public double altitude { get; set; }//= 0.0; // en metros        32.94681944444444         60.6393194444444
                                        // {cityRosario} latitude = -32.94681944, longitude = -60.63931944   (stellarium)
                                        // {cityRosario} latitude = -32.9575, longitude = -60.639444   (internet)
    public static ObserverCoordinates cityRosario = new ObserverCoordinates() { id = 1, name = "Rosario", latitude = -32.94681944444444, longitude = -60.6393194444444, altitude = 0 }; //latitude = -32.9575, longitude = -60.639444
    public static ObserverCoordinates cityQuito = new ObserverCoordinates() { id = 2, name = "Quito", latitude = -0.22, longitude = -78.5125, altitude = 0 };
    public static ObserverCoordinates cityAtenas = new ObserverCoordinates() { id = 3, name = "Atenas", latitude = 37.984167, longitude = 23.728056, altitude = 0 };
}
public class HorizontalCoordinates
{
    public double Altitude { get; set; }
    public double Azimuth { get; set; }
}
public class EquatorialCoordinates
{
    public int idHD { get; set; }
    public double ra { get; set; } // en horas
    public double dec { get; set; }// en grados
    public double _epoch = 2000.0; // en años julianos
    public double epoch { get { return _epoch; } set { _epoch = value; } }

    // public double ra_radianes { get { return ra * Math.PI / 180.0; } }
    //public double dec_radianes { get { return dec * Math.PI / 180.0; } }
    //public EquatorialCoordinates sirio_eq = new EquatorialCoordinates() { dec = sirio_Dec, ra = sirio_RA };

}
public class HorariasCoordinates
{
    public double dec { get; set; } // en grados
    public double HA { get; set; } // en grados
}
public class ServoCoordinates
{
    private double _servoH { get; set; }
    private double _servoV { get; set; }
    public double servoH { get { return _servoH; } set { if (value >= 0.0 && value <= 180.0) { _servoH = value; } } }
    public double servoV { get { return _servoV; } set { if (value >= 0.0 && value <= 180.0) { _servoV = value; } } }


    public static ServoCoordinates convertServoCoordinates(HorizontalCoordinates pValue)
    {
        bool isAzimuthMas180 = false;
        double horizontal = pValue.Azimuth;
        double vertical = pValue.Altitude;// si es negativo
        /*if (pValue.Altitude < 0.0)
        {
            return null;
        }*/
        if (pValue.Azimuth < 180.0)
        {
            isAzimuthMas180 = true;
            horizontal = 180.0 - pValue.Azimuth;
        }
        else
        {
            horizontal = 360.0 - pValue.Azimuth;
        }

        if (isAzimuthMas180)
        {
            vertical = 180.0 - pValue.Altitude;
        }
        return new ServoCoordinates() { servoH = Math.Round(horizontal, 6), servoV = Math.Round(vertical, 6) };
    }
}