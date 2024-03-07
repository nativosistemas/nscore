using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace nscore;

public class Singleton_SessionApp
{
    private static readonly Lazy<Singleton_SessionApp> instance = new Lazy<Singleton_SessionApp>(() => new Singleton_SessionApp());

    private Singleton_SessionApp()
    {
        publicID = Guid.NewGuid();
    }
    public Guid publicID { get; set; }
    public static Singleton_SessionApp Instance
    {
        get
        {
            return instance.Value;
        }
    }
}
[Index(nameof(publicID), IsUnique = true)]
public class SessionApp
{
    public SessionApp()
    {

    }
    public SessionApp(string pName)
    {

        publicID = Singleton_SessionApp.Instance.publicID;//Guid.NewGuid();
        name = pName;
        createDate = DateTime.Now;
    }
    [Key]
    public Guid publicID { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public string name { get; set; }
    public DateTime createDate { get; set; }
}
[Index(nameof(publicID), IsUnique = true)]
public class SessionDevice
{
    public SessionDevice()
    {

    }
    public SessionDevice(Guid pSessionApp_publicID, Guid pDevice_publicID, string pDevice_name)
    {
        publicID = Guid.NewGuid();
        device_name = pDevice_name;
        device_publicID = pDevice_publicID;
        sessionApp_publicID = pSessionApp_publicID;
        createDate = DateTime.Now;
    }
    [Key]
    public Guid publicID { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public Guid device_publicID { get; set; }
    public Guid sessionApp_publicID { get; set; }
    public string device_name { get; set; }
    public DateTime createDate { get; set; }
}

public class StellariumStar_base
{
    public double? ra { get; set; }
    public double? raJ2000 { get; set; }
    public double? dec { get; set; }
    public double? decJ2000 { get; set; }
    public string? star_type { get; set; }
    public string? type { get; set; }
    public double? absolute_mag { get; set; }
    public string? iauConstellation { get; set; }
    public string? name { get; set; }
    public string? localized_name { get; set; }
    public double? distance_ly { get; set; }
    public string? object_type { get; set; }
    public double? parallax { get; set; }
    public string? spectral_class { get; set; }
    public string? variable_star { get; set; }

}
public class StellariumConstellation_base
{
    public string? type { get; set; }
    public string? iauConstellation { get; set; }
    public string? name { get; set; }
    public string? localized_name { get; set; }

}
[Index(nameof(publicID), IsUnique = true)]
public class StellariumConstellation : StellariumConstellation_base
{
    public StellariumConstellation()
    {
        publicID = Guid.NewGuid();
    }
    [Key]
    public Guid publicID { get; set; }
}
[Index(nameof(publicID), IsUnique = true)]
public class StellariumStar : StellariumStar_base
{
    public StellariumStar()
    {
        publicID = Guid.NewGuid();
    }
    [Key]
    public Guid publicID { get; set; }
    public int? hip { get; set; }
    public string? name_web { get; set; }
    public string getName()
    {
        string result = string.Empty;
        if (!string.IsNullOrEmpty(name_web))
        {
            result = name_web;
        }
        else if (!string.IsNullOrEmpty(name))
        {
            result = name;
        }
        else
        {
            result = publicID.ToString();
        }
        return result;
    }
}

[Index(nameof(publicID), IsUnique = true)]
public class AntTracking
{
    public AntTracking()
    {

    }
    public AntTracking(Guid pPublicID, string pType, double? pRa_h = null, double? pDec_v = null)
    {
        publicID = pPublicID;
        type = pType;
        date = DateTime.Now;
        status = Constantes.astro_status_create;
        if (pType == Constantes.astro_type_star)
        {
            ra = pRa_h;
            dec = pDec_v;
        }
        else if (pType == Constantes.astro_type_servoAngle)
        {
            h = pRa_h;
            v = pDec_v;
        }
        else if (pType == Constantes.astro_type_servoAngle_inicio)
        {
            h = 0;
            v = 0;
            status = Constantes.astro_status_calculationResolution;
        }

    }
    [Key]
    public Guid publicID { get; set; }
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public Guid sessionDevice_publicID { get; set; }
    public Guid sessionApp_publicID { get; set; }
    public string type { get; set; }
    public DateTime date { get; set; }
    public double? ra { get; set; }
    public double? dec { get; set; }
    public double? altitude { get; set; }
    public double? azimuth { get; set; }
    public double? h { get; set; }
    public double? v { get; set; }
    public string? info { get; set; }
    //public DateTime? dateProcess { get; set; }
    public bool tracking { get; set; }
    public string status { get; set; }// 1 = creado // 2 = realizar calculos // 3 = se movio servo
    public DateTime? statusUpdateDate { get; set; }


}

[Index(nameof(name), IsUnique = true)]
public class Config
{
    public Config()
    {
        //publicID = Guid.NewGuid();
    }
    [Key]
    [Required]
    public string name { get; set; }
    public string? value { get; set; }
    public int? valueInt { get; set; }
    public double? valueDouble { get; set; }
}

public class ConfigAnt
{
    public ConfigAnt()
    {
        //publicID = Guid.NewGuid();
    }


    public double latitude { get; set; }
    public double longitude { get; set; }
    public double altitude { get; set; }
    public double horizontal_grados_min { get; set; }//= Math.Round(2.9, 6);
    public double horizontal_grados_max { get; set; }//= Math.Round(12.7, 6);
    public double vertical_grados_min { get; set; }//= Math.Round(2.5, 6);
    public double vertical_grados_max { get; set; }// = Math.Round(12.2, 6);

    public static ConfigAnt configDefault = new ConfigAnt() { latitude = -32.94681944444444, longitude = -60.6393194444444, horizontal_grados_min = Math.Round(2.9, 6), horizontal_grados_max = Math.Round(12.7, 6), vertical_grados_min = Math.Round(2.5, 6), vertical_grados_max = Math.Round(12.2, 6) };
}

public class Esp32_astro
{
    // public int idHIP { get; set; }
    public Guid publicID { get; set; }
    public double horizontal_grados { get; set; }
    public double vertical_grados { get; set; }
    public double horizontal_grados_min { get; set; }
    public double horizontal_grados_max { get; set; }
    public double vertical_grados_min { get; set; }
    public double vertical_grados_max { get; set; }
}

/*
[Index(nameof(publicID), IsUnique = true)]
public class AstroTracking
{

    public AstroTracking()
    {

    }
    public AstroTracking(Guid pPublicID, double pRa, double pDec)
    {
        publicID = pPublicID;
        fecha = DateTime.Now;
        ra = pRa;
        dec = pDec;
        estado = 1;
    }
    public Guid publicID { get; set; }
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public int id { get; set; }
    public DateTime fecha { get; set; }
    public double? ra { get; set; }
    public double? dec { get; set; }
    public double? Altitude { get; set; }
    public double? Azimuth { get; set; }
    public string? info { get; set; }
    public int estado { get; set; }


}
*/
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
    public int id { get; set; }
    public int nameBayer { get; set; }
    public string name { get; set; }
    public double ra { get; set; }
    public double dec { get; set; }
    public bool visible { get; set; }
    public bool nearZenith { get; set; }
    public int hip { get; set; }
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
        // bool isAzimuthMenos180 = false;
        double horizontal = pValue.Azimuth;
        double vertical = pValue.Altitude;
        if (pValue.Azimuth < 180.0)
        {
            // isAzimuthMenos180 = true;
            horizontal = 180.0 - pValue.Azimuth;
            vertical = 180.0 - pValue.Altitude;
        }
        else
        {
            horizontal = 360.0 - pValue.Azimuth;
        }

        /*if (isAzimuthMenos180)
        {
            vertical = 180.0 - pValue.Altitude;
        }*/
        return new ServoCoordinates() { servoH = Math.Round(horizontal, 6), servoV = Math.Round(vertical, 6) };
    }
}