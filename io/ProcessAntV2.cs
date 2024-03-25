using System.Diagnostics;

namespace nscore;
public class ProcessAntV2 : IDisposable
{
    private bool disposedValue = false;
    private PoolEsp32 _poolEsp32 = new PoolEsp32(1);
    private List<Star> _l_Star = null;
    private List<ObserverCoordinates> _l_City = null;
    private ObserverCoordinates _city = ObserverCoordinates.cityRosario;
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
        _l_Star = nscore.Util.getAllStars_stellarium();
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
        }
        return _l_Star.Where(x => x.visible).ToList();
    }
    public string actionAnt_laser(int pIsRead, int pLaser)
    {
        return _poolEsp32.Start_laser(pIsRead, pLaser);
    }
    public string actionAnt_servo(double pHorizontal, double pVertical)
    {
        return _poolEsp32.Start_servoAngle(pHorizontal, pVertical);
    }
    public string findStar(int pId, bool isLaserOn = false)
    {
        string result = string.Empty;
        Star oStar = _l_Star.Where(x => x.id == pId).FirstOrDefault();
        if (oStar != null)
        {
            result = _poolEsp32.Start_star(oStar, isLaserOn);
        }
        else
        {
            result = "No se encontro estrella";
        }
        return result;
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
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _poolEsp32.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class PoolEsp32 : IDisposable
{
    private bool disposedValue = false;
    private readonly Queue<ProcessEsp32> recursosDisponibles;
    private readonly int maxRecursos;
    public PoolEsp32(int pMaxRecursos)
    {
        this.maxRecursos = pMaxRecursos;
        recursosDisponibles = new Queue<ProcessEsp32>();
        for (int i = 0; i < maxRecursos; i++)
        {
            ProcessEsp32 oProcessEsp32 = new ProcessEsp32();
            recursosDisponibles.Enqueue(oProcessEsp32);
        }
    }
    public string Start_laser(int pIsRead, int pLaser)
    {
        string output = "null";
        try
        {
            ProcessEsp32 oProcess = GetResource();
            if (oProcess != null)
            {
                output = oProcess.actionAnt_laser(pIsRead, pLaser);
                SetResource(oProcess);
            }
            else
            {
                output = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return output;
    }
    public string Start_servoAngle(double pHorizontal, double pVertical)
    {
        string output = "null";
        try
        {
            ProcessEsp32 oProcess = GetResource();
            if (oProcess != null)
            {
                output = oProcess.actionAnt_servoAngle(pHorizontal, pVertical);
                SetResource(oProcess);
            }
            else
            {
                output = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return output;
    }
    public string Start_star(Star pStar, bool isLaserOn = false)
    {
        string output = "null";
        try
        {
            ProcessEsp32 oProcess = GetResource();
            if (oProcess != null)
            {
                output = oProcess.actionAnt_star(pStar, isLaserOn);
                SetResource(oProcess);
            }
            else
            {
                output = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return output;
    }
    public ProcessEsp32 GetResource()
    {
        if (recursosDisponibles.Count > 0)
        {
            return recursosDisponibles.Dequeue();
        }

        // Aquí puedes implementar la lógica para manejar el caso en el que no haya recursos disponibles,
        // como lanzar una excepción o esperar hasta que haya un recurso disponible.

        return null;
    }

    public void SetResource(ProcessEsp32 recurso)
    {
        if (recursosDisponibles.Count < maxRecursos)
        {
            recursosDisponibles.Enqueue(recurso);
        }
        else
        {
            // Aquí puedes implementar la lógica para manejar el caso en el que el pool está lleno
            // y no se puede devolver el recurso.
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                //
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}
public class ProcessEsp32 : IDisposable
{
    private bool disposedValue = false;
    public ProcessEsp32()
    {

    }
    public string actionAnt_laser(int pIsRead, int pLaser)
    {
        string result = string.Empty;
        Guid oAstroTracking = Util.newAstroTracking_laser(Constantes.astro_type_laser, pLaser);
        HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(Constantes.astro_type_laser, oAstroTracking).Result;
        if (hc != null)
        {
            result = "Ok";
        }
        return result;
    }
    public string actionAnt_servoAngle(double pHorizontal, double pVertical)
    {
        string result = string.Empty;
        Guid oAstroTracking = Util.newAstroTracking(Constantes.astro_type_servoAngle, pHorizontal, pVertical);
        HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(Constantes.astro_type_servoAngle, oAstroTracking).Result;
        if (hc != null)
        {
            result = "Ok";
        }
        return result;
    }
    public string actionAnt_star(Star pStar, bool isLaserOn = false)
    {
        string result = string.Empty;
        if (pStar != null)
        {
            Guid oAstroTracking = Util.newAstroTracking(Constantes.astro_type_star, pStar.ra, pStar.dec);
            HorizontalCoordinates hc = getAstroTracking_HorizontalCoordinates(Constantes.astro_type_star, oAstroTracking).Result;
            if (hc != null)
            {
                ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(hc);
                if (oServoCoordinates != null)
                {
                    string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(pStar.ra) + "/" + AstronomyEngine.GetSexagesimal(pStar.dec);
                    string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(hc.Altitude);
                    result += strEq + "<br/>" + strHc + "<br/>";
                    result += "HIP " + pStar.hip.ToString() + "<br/>";
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
    public async Task<HorizontalCoordinates> getAstroTracking_HorizontalCoordinates(string pType, Guid pGuid)
    {
        HorizontalCoordinates resault = null;
        int contador = 0;

        while (contador < 300)
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
                    else if (pType == Constantes.astro_type_laser)
                    {
                        resault = new HorizontalCoordinates() { Altitude = 0, Azimuth = 0 };
                    }
                    break;
                }
            }
            await Task.Delay(50);
            contador++;
        }
        return resault;
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // _processLaser.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }
}