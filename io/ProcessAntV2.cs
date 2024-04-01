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
    public async Task<string> actionAnt_laser(int pIsRead, int pLaser)
    {
        return _poolEsp32.Start_laser(pIsRead, pLaser);
    }
    public string actionAnt_servo(double pHorizontal, double pVertical)
    {
        return _poolEsp32.Start_servoAngle(pHorizontal, pVertical);
    }
    public async Task<string> actionAnt_star(int pId, bool isLaserOn = false)
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
    public async Task<Esp32_astro> actionAnt_getAntTracking(string pDevice_publicID, string pSessionDevice_publicID)
    {
        Esp32_astro result = null;
        try
        {
            Guid sessionDevice_publicID = new Guid(pSessionDevice_publicID);
            Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
            using (var context = new AstroDbContext())
            {
                Guid sessionDevice_publicID_return = sessionDevice_publicID;
                SessionDevice o = context.SessionDevices.Where(x => x.publicID == sessionDevice_publicID).FirstOrDefault();//&& x.sessionApp_publicID == sessionApp_publicID
                bool is_sessionDeviceAdd = false;
                if (o == null || o.sessionApp_publicID != sessionApp_publicID)
                {
                    sessionDevice_publicID_return = await sessionDeviceAdd(pDevice_publicID, Constantes.device_name_esp32_servos_laser);
                    is_sessionDeviceAdd = true;
                }
                result = await esp32_getAstro();
                if (result != null && is_sessionDeviceAdd)
                {
                    result.horizontal_grados_ant = null;
                    result.vertical_grados_ant = null;
                }
                result.sessionDevice_publicID_return = sessionDevice_publicID_return;
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public async Task<Guid> sessionDeviceAdd(string pDevice_publicID, string pDevice_name)
    {
        Guid result = Guid.Empty;
        try
        {
            Guid device_publicID = new Guid(pDevice_publicID);
            Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
            using (var context = new AstroDbContext())
            {
                SessionDevice o = new SessionDevice();
                o.device_name = pDevice_name;
                o.device_publicID = device_publicID;
                o.sessionApp_publicID = sessionApp_publicID;
                o.createDate = DateTime.Now;
                result = o.publicID;
                context.SessionDevices.Add(o);
                context.SaveChanges();
            }
            Guid newAntTracking_inicio = await antTracking_resetSession();
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public async Task<Guid> antTracking_resetSession()
    {
        Guid result = Guid.Empty;
        try
        {
            //Guid sessionDevice_publicID = new Guid(pSessionDevice_publicID);
            Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
            using (var context = new AstroDbContext())
            {
                //Guid sessionApp_publicID = oSessionDevices.sessionApp_publicID;
                List<nscore.AntTracking> l = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_movingServo).ToList();
                DateTime dateNow = DateTime.Now;
                foreach (var oItem in l)
                {
                    oItem.status = Constantes.astro_status_resetSession;
                    oItem.statusUpdateDate = dateNow;
                }
                context.SaveChanges();
                //Guid publicID = Util.newAstroTracking(Constantes.astro_type_servoAngle_inicio, 0, 0);
                //result = publicID;

            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public async Task<Esp32_astro> esp32_getAstro()
    {
        Esp32_astro result = null;
        try
        {
            using (var context = new AstroDbContext())
            {
                Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
                AntTracking oAntTracking = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_calculationResolution).OrderBy(x1 => x1.date).FirstOrDefault();
                if (oAntTracking != null)
                {
                    AntTracking oAntTracking_ant = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_movedServo && x.statusUpdateDate != null).OrderByDescending(x1 => x1.statusUpdateDate.Value).FirstOrDefault();
                    double? h_old = null;
                    double? v_old = null;
                    double? h_diferencia_grados = null;
                    double? v_diferencia_grados = null;
                    if (oAntTracking_ant != null)
                    {
                        h_old = oAntTracking_ant.h;
                        v_old = oAntTracking_ant.v;
                        if (h_old != null && oAntTracking.h != null)
                        {
                            if (h_old.Value > oAntTracking.h.Value)
                            {
                                h_diferencia_grados = Math.Abs(h_old.Value - oAntTracking.h.Value);
                            }
                            else
                            {
                                h_diferencia_grados = Math.Abs(oAntTracking.h.Value - h_old.Value);
                            }
                        }
                        if (v_old != null && oAntTracking.v != null)
                        {
                            if (v_old.Value > oAntTracking.v.Value)
                            {
                                v_diferencia_grados = Math.Abs(v_old.Value - oAntTracking.v.Value);
                            }
                            else
                            {
                                v_diferencia_grados = Math.Abs(oAntTracking.v.Value - v_old.Value);
                            }
                        }
                    }
                    double h_sleep_secs = Constantes.servo_sleep_max;
                    double v_sleep_secs = Constantes.servo_sleep_max;
                    if (h_diferencia_grados != null)
                    {
                        h_sleep_secs = double.Round((h_sleep_secs * h_diferencia_grados.Value) / 180.0, 1);
                        if (h_sleep_secs < Constantes.servo_sleep_min)
                        {
                            h_sleep_secs = 0.5;
                        }
                    }
                    if (v_diferencia_grados != null)
                    {
                        v_sleep_secs = double.Round((v_sleep_secs * v_diferencia_grados.Value) / 180.0, 1);
                        if (v_sleep_secs < Constantes.servo_sleep_min)
                        {
                            v_sleep_secs = 0.5;
                        }
                    }

                    await AntTrackingStatus(oAntTracking.publicID, Constantes.astro_status_movingServo, null);
                    //
                    List<Config> l = context.Configs.ToList();

                    double _Horizontal_grados_min = 0;
                    double _Horizontal_grados_max = 0;
                    double _Vertical_grados_min = 0;
                    double _Vertical_grados_max = 0;

                    if (l != null)
                    {
                        _Horizontal_grados_min = l.FirstOrDefault(x => x.name == "horizontal_grados_min").valueDouble.Value;
                        _Horizontal_grados_max = l.FirstOrDefault(x => x.name == "horizontal_grados_max").valueDouble.Value;
                        _Vertical_grados_min = l.FirstOrDefault(x => x.name == "vertical_grados_min").valueDouble.Value;
                        _Vertical_grados_max = l.FirstOrDefault(x => x.name == "vertical_grados_max").valueDouble.Value;
                    }

                    //
                    result = new Esp32_astro()
                    {
                        type = oAntTracking.type,
                        isLaser = oAntTracking.isLaser,
                        publicID = oAntTracking.publicID,
                        horizontal_grados = oAntTracking.h == null ? 0 : oAntTracking.h.Value,
                        vertical_grados = oAntTracking.v == null ? 0 : oAntTracking.v.Value,
                        horizontal_grados_ant = h_old,
                        vertical_grados_ant = v_old,
                        horizontal_grados_sleep = h_sleep_secs,
                        vertical_grados_sleep = v_sleep_secs,
                        horizontal_grados_min = _Horizontal_grados_min,
                        horizontal_grados_max = _Horizontal_grados_max,
                        vertical_grados_min = _Vertical_grados_min,
                        vertical_grados_max = _Vertical_grados_max

                    };
                }
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public async Task<string> esp32_setAstro(string pPublicID, string pSessionDevice_publicID)
    {
        string result = string.Empty;
        try
        {
            Guid publicID = new Guid(pPublicID);
            Guid sessionDevice_publicID = new Guid(pSessionDevice_publicID);
            await AntTrackingStatus(publicID, Constantes.astro_status_movedServo, sessionDevice_publicID);
            result = "Ok";
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public static async Task<bool> AntTrackingStatus(Guid pGuid, string pEstado, Guid? pSessionDevice_publicID)
    {
        bool result = false;
        using (var context = new AstroDbContext())
        {
            AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();
            if (oAntTracking != null)
            {
                oAntTracking.status = pEstado;
                oAntTracking.statusUpdateDate = DateTime.Now;
                if (pSessionDevice_publicID != null)
                {
                    oAntTracking.sessionDevice_publicID = pSessionDevice_publicID.Value;
                }
                context.SaveChanges();
                result = true;
            }
        }
        return result;
    }
    public async Task<bool> removeTable()
    {
        bool result = false;
        using (var context = new AstroDbContext())
        {
            context.AntTrackings.RemoveRange(context.AntTrackings.ToList());
            context.SessionApps.RemoveRange(context.SessionApps.ToList());
            context.SessionDevices.RemoveRange(context.SessionDevices.ToList());
            context.SaveChanges();
            result = true;
        }
        return result;
    }
    public async Task<string> getValoresServos()
    {
        try
        {
            using (var context = new AstroDbContext())
            {
                List<Config> l = context.Configs.ToList();

                Guid sessionApp_publicID = Singleton_SessionApp.Instance.publicID;
                AntTracking oAntTracking = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_movedServo && x.statusUpdateDate != null).OrderByDescending(x1 => x1.statusUpdateDate.Value).FirstOrDefault();
                if (oAntTracking != null)
                {
                    _Horizontal_grados = oAntTracking.h.Value;
                    _Vertical_grados = oAntTracking.v.Value;
                }
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
    public async Task<string> setConfig(double latitude, double longitude, double horizontal_grados_min, double horizontal_grados_max, double vertical_grados_min, double vertical_grados_max)
    {
        string result = "!Ok";
        try
        {
            using (var context = new AstroDbContext())
            {
                List<Config> l = context.Configs.ToList();
                l.FirstOrDefault(x => x.name == "latitude").valueDouble = latitude;
                l.FirstOrDefault(x => x.name == "longitude").valueDouble = longitude;
                l.FirstOrDefault(x => x.name == "horizontal_grados_min").valueDouble = horizontal_grados_min;
                l.FirstOrDefault(x => x.name == "horizontal_grados_max").valueDouble = horizontal_grados_max;
                l.FirstOrDefault(x => x.name == "vertical_grados_min").valueDouble = vertical_grados_min;
                l.FirstOrDefault(x => x.name == "vertical_grados_max").valueDouble = vertical_grados_max;
                context.SaveChanges();
            }
            result = "Ok";
        }
        catch (Exception ex)
        {
            nscore.Util.log(ex);
        }
        return result;
    }
    public async Task<ConfigAnt> getConfig()
    {
        ConfigAnt result = null;
        try
        {
            using (var context = new AstroDbContext())
            {
                List<Config> l = context.Configs.ToList();
                double _Horizontal_grados_min = l.FirstOrDefault(x => x.name == "horizontal_grados_min").valueDouble.Value;
                double _Horizontal_grados_max = l.FirstOrDefault(x => x.name == "horizontal_grados_max").valueDouble.Value;
                double _Vertical_grados_min = l.FirstOrDefault(x => x.name == "vertical_grados_min").valueDouble.Value;
                double _Vertical_grados_max = l.FirstOrDefault(x => x.name == "vertical_grados_max").valueDouble.Value;
                double latitude = l.FirstOrDefault(x => x.name == "latitude").valueDouble.Value;
                double longitude = l.FirstOrDefault(x => x.name == "longitude").valueDouble.Value;


                result = new ConfigAnt() { latitude = latitude, longitude = longitude, horizontal_grados_min = _Horizontal_grados_min, horizontal_grados_max = _Horizontal_grados_max, vertical_grados_min = _Vertical_grados_min, vertical_grados_max = _Vertical_grados_max };
            }
        }
        catch (Exception ex)
        {
            nscore.Util.log(ex);
        }
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
        else
        {
            result = "No se obtuvo respuesta";
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
        bool isFoundAntTracking = false;
        while (contador < 400)
        {
            using (var context = new AstroDbContext())
            {
                AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid && x.status == Constantes.astro_status_movedServo).FirstOrDefault();
                if (oAntTracking != null)
                {
                    isFoundAntTracking = true;
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
        if (!isFoundAntTracking)
        {
            await ProcessAntV2.AntTrackingStatus(pGuid, Constantes.astro_status_noResponseEsp32, null);
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