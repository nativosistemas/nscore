using System.Diagnostics;

namespace nscore;
public class ProcessAntV2 : IDisposable
{
    private bool disposedValue = false;
    //private Process _controller = new Process();
    // private bool _versionNew = true;
    private ProcessServo _processServo = new ProcessServo();
    private ProcessServoRango _processServoRango = new ProcessServoRango();
    private ProcessCoordinatesStar _processCoordinatesStar = new ProcessCoordinatesStar();
    private ProcessLaser _processLaser = new ProcessLaser();
    //private Process _controllerLaser = new Process();
    private List<Star> _l_Star = null;
    private List<ObserverCoordinates> _l_City = null;
    //  private List<Constellation> _l_Constellation = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
    //private static Semaphore _semaphore_ant = new Semaphore(0, 1);
    public ObserverCoordinates city { get { return _city; } set { _city = value; } }

    public double _Horizontal_grados = 0;
    public double _Vertical_grados = 0;
    public double _Horizontal_grados_min = 0;
    public double _Horizontal_grados_max = 0;
    public double _Vertical_grados_min = 0;
    public double _Vertical_grados_max = 0;

    public ProcessAntV2()
    {
        _city = getCitys().FirstOrDefault();
        /*_l_Star = new List<Star>();
        List<AstronomicalObject> l = nscore.Util.getAstronomicalObjects().Where(x => x.magnitudAparente != null).OrderBy(x => x.magnitudAparente).ToList();
        //foreach (AstronomicalObject oStar in l){
        for (int i = 0; i < l.Count; i++)
        {
            AstronomicalObject o = l[i];
            Star oStar = new Star();
            oStar.id = o.idHD;
            oStar.dec = o.dec.Value;
            oStar.ra = o.ra.Value;
            oStar.name = o.getName();
            oStar.idHD = o.idHD;
            _l_Star.Add(oStar);
        }*/
        _l_Star = nscore.Util.getAllStars_stellarium();
        // _l_Constellation = nscore.Util.getConstelaciones();
    }
    public ObserverCoordinates setCity(int id, string pName, double pLatitude, double pLongitude)
    {
        _city = new ObserverCoordinates() { id = id, name = pName, latitude = pLatitude, longitude = pLongitude, altitude = 0 };
        return _city;
    }
    public List<ObserverCoordinates> getCitys()
    {
        if (_l_City == null)
        {
            _l_City = new List<ObserverCoordinates>();
            _l_City.Add(ObserverCoordinates.cityRosario);
            _l_City.Add(ObserverCoordinates.cityQuito);
            _l_City.Add(ObserverCoordinates.cityAtenas);
        }
        return _l_City;
    }
    public List<Star> getStars()
    {
        double siderealTime_local = AstronomyEngine.GetTSL(DateTime.UtcNow, city);
        foreach (Star oStar in _l_Star)
        {
            EquatorialCoordinates eq = new EquatorialCoordinates() { dec = oStar.dec, ra = oStar.ra };
            HorizontalCoordinates hc = AstronomyEngine.ToHorizontalCoordinates(siderealTime_local, city, eq);
            ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
            if (hc.Altitude > 40)
            {
                oStar.nearZenith = true;
            }
            else
            {
                oStar.nearZenith = false;
            }
            if (hc.Altitude < 1.0)
            {
                oStar.visible = false;
            }
            else
            {
                oStar.visible = true;
            }
            // oStar.visible = true;
        }
        return _l_Star.Where(x => x.visible).ToList();
    }
    public string actionAnt_servo(double pHorizontal, double pVertical)
    {
        string result = string.Empty;
        Guid oAstroTracking = saveAstroTracking(Constantes.astro_type_servoAngle, pHorizontal, pVertical);
        HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(Constantes.astro_type_servoAngle, oAstroTracking).Result;
        if (hc != null)
        {
            result = "Ok";
        }
        return result;
    }
    public string findStar(int pId, bool isLaserOn = false)
    {
        string result = string.Empty;
        Star oStar = _l_Star.Where(x => x.id == pId).FirstOrDefault();
        if (oStar != null)
        {
            Guid oAstroTracking = saveAstroTracking(Constantes.astro_type_star, oStar.ra, oStar.dec);
            HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(Constantes.astro_type_star, oAstroTracking).Result;
            //removeAstroTracking(oAstroTracking);
            if (hc != null)
            {
                ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
                if (oServoCoordinates != null)
                {
                    string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(oStar.ra) + "/" + AstronomyEngine.GetSexagesimal(oStar.dec);
                    string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(hc.Altitude);
                    result += strEq + "<br/>" + strHc + "<br/>";
                    result += "HIP " + oStar.hip.ToString() + "<br/>";
                }
                else
                {
                    result = "Estrella no es visible";
                }
            }
            else
            {
                result = "No se obtuvo respuesta";
            }
        }
        else
        {
            result = "No se encontro estrella";
        }
        return result;
    }
    public Guid saveAstroTracking(string pType, double pRa_h, double pDec_v)
    {
        Guid oGuid = Guid.NewGuid();
        using (var context = new AstroDbContext())
        {

            nscore.AntTracking o = new nscore.AntTracking(oGuid, pType, pRa_h, pDec_v);
            context.AntTrackings.Add(o);
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                nscore.Util.log(ex);
            }
        }
        return oGuid;
    }
    public async Task<HorizontalCoordinates> getAstroTracking_HorizontalCoordinates(string pType, Guid pGuid)
    {
        HorizontalCoordinates resault = null;
        int contador = 0;

        while (contador < 100)
        {
            using (var context = new AstroDbContext())
            {
                AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid && x.status == Constantes.astro_status_movedServo).FirstOrDefault();
                if (oAntTracking != null)
                {
                    if (pType == Constantes.astro_type_star)
                    {
                        resault = new HorizontalCoordinates() { Altitude = oAntTracking.altitude.Value, Azimuth = oAntTracking.azimuth.Value };
                    }
                    else if (pType == Constantes.astro_type_servoAngle)
                    {
                        resault = new HorizontalCoordinates() { Altitude = oAntTracking.h.Value, Azimuth = oAntTracking.v.Value };
                    }
                    break;
                }
            }
            await Task.Delay(200);
            contador++;
        }
        return resault;
    }
    public string getValoresServos()
    {
        try
        {
            using (var context = new AstroDbContext())
            {
                List<Config> l = context.Configs.ToList();
                _Horizontal_grados = l.FirstOrDefault(x => x.name == "servoH").valueDouble.Value;
                _Vertical_grados = l.FirstOrDefault(x => x.name == "servoV").valueDouble.Value;
                _Horizontal_grados_min = l.FirstOrDefault(x => x.name == "horizontal_grados_min").valueDouble.Value;
                _Horizontal_grados_max = l.FirstOrDefault(x => x.name == "horizontal_grados_max").valueDouble.Value;
                _Vertical_grados_min = l.FirstOrDefault(x => x.name == "vertical_grados_min").valueDouble.Value;
                _Vertical_grados_max = l.FirstOrDefault(x => x.name == "vertical_grados_max").valueDouble.Value;
            }
        }
        catch (Exception ex)
        {
            nscore.Util.log(ex);
        }
        string result = "{ \"horizontal\":" + _Horizontal_grados.ToString(System.Globalization.CultureInfo.InvariantCulture) +
        ", \"vertical\":" + _Vertical_grados.ToString(System.Globalization.CultureInfo.InvariantCulture) +
          ", \"horizontal_min\":" + _Horizontal_grados_min.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", \"horizontal_max\":" + _Horizontal_grados_max.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                              ", \"vertical_min\":" + _Vertical_grados_min.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                    ", \"vertical_max\":" + _Vertical_grados_max.ToString(System.Globalization.CultureInfo.InvariantCulture) +
        " }";
        return result;
    }
    
    /*
    public bool removeAstroTracking(Guid pGuid)
    {
        bool result = false;
        using (var context = new AstroDbContext())
        {
            AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();


            if (oAntTracking != null)
            {
                context.AntTrackings.Remove(oAntTracking);
                context.SaveChanges();
                result = true;
            }
        }
        return result;
    }
    */
    /*public string moveTheAnt(ServoCoordinates pServoCoordinates)
    {
        return _processServo.Start(pServoCoordinates.servoH, pServoCoordinates.servoV, 1);
    }*/
    /*
    public string moveTheAnt_rango(ServoCoordinates pServoCoordinates, bool isLaserOn = false)
    {
        return moveTheAnt_rango(pServoCoordinates.servoH, pServoCoordinates.servoV, _Horizontal_grados_min, _Horizontal_grados_max, _Vertical_grados_min, _Vertical_grados_max, isLaserOn ? 1 : 0);
    }
    public string moveTheAnt_rango(double pH, double pV, double pH_min, double pH_max, double pV_min, double pV_max, int pLaser)
    {
        double horizontal = 0;
        double vertical = 0;
        if (_Horizontal_grados > pH)
        {
            horizontal = Math.Abs(_Horizontal_grados - pH);
        }
        else
        {
            horizontal = Math.Abs(pH - _Horizontal_grados);
        }
        if (_Vertical_grados > pV)
        {
            vertical = Math.Abs(_Vertical_grados - pV);
        }
        else
        {
            vertical = Math.Abs(pV - _Vertical_grados);
        }
        _Horizontal_grados = pH;
        _Vertical_grados = pV;
        _Horizontal_grados_min = pH_min;
        _Horizontal_grados_max = pH_max;
        _Vertical_grados_min = pV_min;
        _Vertical_grados_max = pV_max;
        double sleep_secs = 3;
        if (horizontal > vertical)
        {
            sleep_secs = double.Round((sleep_secs * horizontal) / 180.0, 1);
        }
        else
        {
            sleep_secs = double.Round((sleep_secs * vertical) / 180.0, 1);
        }
        if (sleep_secs < 0.5)
        {
            sleep_secs = 0.5;
        }

        return _processServoRango.Start(pH, pV, pH_min, pH_max, pV_min, pV_max, pLaser, sleep_secs);
    }
    public string actionLaser(int pIsRead, int pLaser)
    {
        return _processLaser.Start(pIsRead, pLaser);
    }*/
    /*public string actionGrabarSirio()
    {
        Guid? oAstroTracking = null;
        Star oStar = _l_Star.Where(x => x.id == 48915).FirstOrDefault();
        if (oStar != null)
        {
            oAstroTracking = saveAstroTracking(Constantes.astro_type_star,oStar.ra, oStar.dec);

        }
        return oAstroTracking == null ? "!Ok" : oAstroTracking.ToString();
    }*/

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _processServo.Dispose();
                _processLaser.Dispose();
                _processServoRango.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}