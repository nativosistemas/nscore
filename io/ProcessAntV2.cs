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
    public double _Horizontal_grados_min = Math.Round(2.9, 6);
    public double _Horizontal_grados_max = Math.Round(12.7, 6);
    public double _Vertical_grados_min = Math.Round(2.5, 6);
    public double _Vertical_grados_max = Math.Round(12.2, 6);

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
        _l_Star = nscore.Util.getAllStars();
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
    public string findStar(int pId, bool isLaserOn = false)
    {
        string result = string.Empty;
        Star oStar = _l_Star.Where(x => x.id == pId).FirstOrDefault();
        if (oStar != null)
        {
            Guid oAstroTracking = saveAstroTracking(oStar.ra, oStar.dec);
            HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(oAstroTracking).Result;
            removeAstroTrackingEstado(oAstroTracking);
            if (hc != null)
            {
                ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
                if (oServoCoordinates != null)
                {
                    string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(oStar.ra) + "/" + AstronomyEngine.GetSexagesimal(oStar.dec);
                    string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(hc.Altitude);
                    result += strEq + "<br/>" + strHc + "<br/>";
                    result += "HD " + oStar.idHD.ToString() + "<br/>";
                    result += "Servo: " + moveTheAnt_rango(oServoCoordinates, isLaserOn);
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
    public Guid saveAstroTracking(double pRa, double pDec)
    {
        Guid oGuid = Guid.NewGuid();
        using (var context = new AstroDbContext())
        {

            nscore.AstroTracking o = new nscore.AstroTracking(oGuid, pRa, pDec);
            context.AstroTrackings.Add(o);

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
    public async Task<HorizontalCoordinates> getAstroTracking_HorizontalCoordinates(Guid pGuid)
    {
        HorizontalCoordinates resault = null;
        using (var context = new AstroDbContext())
        {
            int contador = 0;

            while (contador < 5)
            {
                AstroTracking oAstroTracking = context.AstroTrackings.Where(x => x.publicID == pGuid && x.estado == 2).FirstOrDefault();
                if (oAstroTracking != null)
                {
                    resault = new HorizontalCoordinates() { Altitude = oAstroTracking.Altitude.Value, Azimuth = oAstroTracking.Azimuth.Value };
                    break;
                }
                await Task.Delay(500);
                contador++;
            }
        }
        return resault;
    }
    public bool changeAstroTrackingEstado(Guid pGuid, int pEstado)
    {
        bool result = false;
        // Crear e inicializar el contexto
        using (var context = new AstroDbContext())
        {
            // Buscar el usuario por ID
            AstroTracking oAstroTracking = context.AstroTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();


            if (oAstroTracking != null)
            {
                // Actualizar los valores
                oAstroTracking.estado = pEstado;

                // Guardar los cambios en la base de datos
                context.SaveChanges();
                result = true;
            }
        }
        return result;
    }
    public bool removeAstroTrackingEstado(Guid pGuid)
    {
        bool result = false;
        // Crear e inicializar el contexto
        using (var context = new AstroDbContext())
        {
            // Buscar el usuario por ID
            AstroTracking oAstroTracking = context.AstroTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();


            if (oAstroTracking != null)
            {
                context.AstroTrackings.Remove(oAstroTracking);
                // Guardar los cambios en la base de datos
                context.SaveChanges();
                result = true;
            }
        }
        return result;
    }
    /*public string moveTheAnt(ServoCoordinates pServoCoordinates)
    {
        return _processServo.Start(pServoCoordinates.servoH, pServoCoordinates.servoV, 1);
    }*/
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
    }
    public string actionGrabarSirio()
    {
        Guid? oAstroTracking = null;
        Star oStar = _l_Star.Where(x => x.id == 48915).FirstOrDefault();
        if (oStar != null)
        {
            oAstroTracking = saveAstroTracking(oStar.ra, oStar.dec);

        }
        return oAstroTracking == null ? "!Ok" : oAstroTracking.ToString();
    }

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