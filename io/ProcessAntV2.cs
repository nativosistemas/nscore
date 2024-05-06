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
    public async Task<cResultAnt> actionAnt_laser(int pIsRead, int pLaser)
    {
        return await _poolEsp32.Start_laser(pIsRead, pLaser);
    }
    public async Task<cResultAnt> actionAnt_servo(double pHorizontal, double pVertical)
    {
        return await _poolEsp32.Start_servoAngle(pHorizontal, pVertical);
    }
    public async Task<cResultAnt> actionAnt_star(int pId, bool isLaserOn = false)
    {
        cResultAnt result = new cResultAnt();
        Star oStar = _l_Star.Where(x => x.id == pId).FirstOrDefault();
        if (oStar != null)
        {
            result = _poolEsp32.Start_star(oStar, isLaserOn);
        }
        else
        {
            //result.type = "sinResultado";
            result.msg = "No se encontro estrella";
        }
        return result;
    }
    public async Task<Esp32_astro> actionAnt_getAntTracking(string pDevice_publicID, string pSessionDevice_publicID)
    {
        Esp32_astro result = null;
        try
        {
            if (isDeviceValid(pDevice_publicID).Result)
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
                        is_sessionDeviceAdd = true;
                    }
                    if (is_sessionDeviceAdd)
                    {
                        SessionDevice oSessionDevices = context.SessionDevices.Where(x => x.sessionApp_publicID == sessionApp_publicID).FirstOrDefault();
                        if (oSessionDevices == null)
                        {
                            sessionDevice_publicID_return = await sessionDeviceAdd(pDevice_publicID, Constantes.device_name_esp32_servos_laser);
                        }
                        else
                        {
                            sessionDevice_publicID_return = oSessionDevices.publicID;
                        }

                    }

                    result = await esp32_getAstro();
                    if (result == null)
                    {
                        result = new Esp32_astro();
                    }
                    else if (is_sessionDeviceAdd)
                    {
                        result.horizontal_grados_ant = null;
                        result.vertical_grados_ant = null;
                    }
                    result.sessionDevice_publicID_return = sessionDevice_publicID_return;
                }
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public async Task<Guid> api_sessionDeviceAdd(string pDevice_publicID)
    {
        Guid result = await sessionDeviceAdd(pDevice_publicID, Constantes.device_name_esp32_servos_laser); ;
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
    public async Task<bool> isDeviceValid(string pDevice_publicID)
    {
        bool result = false;
        if (Helper.IoT_esp32 == pDevice_publicID)
        {
            result = true;
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
                    //
                    List<Config> l = context.Configs.ToList();

                    double _Horizontal_grados_min = 0;
                    double _Horizontal_grados_max = 0;
                    double _Vertical_grados_min = 0;
                    double _Vertical_grados_max = 0;
                    double _Horizontal_grados_calibrate = 0;
                    double _Vertical_grados_calibrate = 0;

                    if (l != null)
                    {
                        _Horizontal_grados_min = l.FirstOrDefault(x => x.name == "horizontal_grados_min").valueDouble.Value;
                        _Horizontal_grados_max = l.FirstOrDefault(x => x.name == "horizontal_grados_max").valueDouble.Value;
                        _Vertical_grados_min = l.FirstOrDefault(x => x.name == "vertical_grados_min").valueDouble.Value;
                        _Vertical_grados_max = l.FirstOrDefault(x => x.name == "vertical_grados_max").valueDouble.Value;
                        _Horizontal_grados_calibrate = l.FirstOrDefault(x => x.name == "horizontal_grados_calibrate").valueDouble.Value;
                        _Vertical_grados_calibrate = l.FirstOrDefault(x => x.name == "vertical_grados_calibrate").valueDouble.Value;
                    }
                    if (oAntTracking.type != Constantes.astro_type_servoAngle_calibrate)
                    {
                        oAntTracking._h_calibrate = _Horizontal_grados_calibrate;
                        oAntTracking._v_calibrate = _Vertical_grados_calibrate;
                    }
                    //

                    AntTracking oAntTracking_ant = context.AntTrackings.Where(x => x.sessionApp_publicID == sessionApp_publicID && x.status == Constantes.astro_status_movedServo && x.statusUpdateDate != null).OrderByDescending(x1 => x1.statusUpdateDate.Value).FirstOrDefault();
                    double? h_old = null;
                    double? v_old = null;
                    double? h_diferencia_grados = null;
                    double? v_diferencia_grados = null;
                    if (oAntTracking_ant != null)
                    {
                        h_old = oAntTracking_ant.get_h_calibrate();
                        v_old = oAntTracking_ant.get_v_calibrate();
                        if (h_old != null && oAntTracking.h != null)
                        {
                            if (h_old.Value > oAntTracking.get_h_calibrate())
                            {
                                h_diferencia_grados = Math.Abs(h_old.Value - oAntTracking.get_h_calibrate());
                            }
                            else
                            {
                                h_diferencia_grados = Math.Abs(oAntTracking.get_h_calibrate() - h_old.Value);
                            }
                        }
                        if (v_old != null && oAntTracking.v != null)
                        {
                            if (v_old.Value > oAntTracking.get_v_calibrate())
                            {
                                v_diferencia_grados = Math.Abs(v_old.Value - oAntTracking.get_v_calibrate());
                            }
                            else
                            {
                                v_diferencia_grados = Math.Abs(oAntTracking.get_v_calibrate() - v_old.Value);
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

                    // await AntTrackingStatus(oAntTracking.publicID, Constantes.astro_status_movingServo, null);
                    oAntTracking.status = Constantes.astro_status_movingServo;
                    oAntTracking.statusUpdateDate = DateTime.Now;

                    context.SaveChanges();
                    result = new Esp32_astro()
                    {
                        type = oAntTracking.type,
                        isLaser = oAntTracking.isLaser,
                        publicID = oAntTracking.publicID,
                        horizontal_grados = oAntTracking.get_h_calibrate(),
                        vertical_grados = oAntTracking.get_v_calibrate(),
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
            await AntTrackingStatus_endEsp32(publicID, sessionDevice_publicID);
            result = "Ok";
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public static async Task<bool> AntTrackingStatus_endEsp32(Guid pGuid, Guid? pSessionDevice_publicID)
    {
        bool result = false;
        using (var context = new AstroDbContext())
        {
            AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid).FirstOrDefault();
            string status = Constantes.astro_status_movedServo;
            if (oAntTracking != null)
            {
                if (oAntTracking.type == Constantes.astro_type_laser)
                {
                    status = Constantes.astro_status_movedLaser;
                }
                oAntTracking.status = status;
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
                //oAntTracking.h = oAntTracking.h;
                //oAntTracking.v = oAntTracking.v;
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
            context.Logs.RemoveRange(context.Logs.ToList());
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
    public async Task<string> setConfig(double latitude, double longitude, double horizontal_grados_min, double horizontal_grados_max, double vertical_grados_min, double vertical_grados_max, double horizontal_grados_calibrate, double vertical_grados_calibrate)
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
                l.FirstOrDefault(x => x.name == "horizontal_grados_calibrate").valueDouble = horizontal_grados_calibrate;
                l.FirstOrDefault(x => x.name == "vertical_grados_calibrate").valueDouble = vertical_grados_calibrate;
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
                double horizontal_grados_calibrate = l.FirstOrDefault(x => x.name == "horizontal_grados_calibrate").valueDouble.Value;
                double vertical_grados_calibrate = l.FirstOrDefault(x => x.name == "vertical_grados_calibrate").valueDouble.Value;

                result = new ConfigAnt() { latitude = latitude, longitude = longitude, horizontal_grados_min = _Horizontal_grados_min, horizontal_grados_max = _Horizontal_grados_max, vertical_grados_min = _Vertical_grados_min, vertical_grados_max = _Vertical_grados_max, horizontal_grados_calibrate = horizontal_grados_calibrate, vertical_grados_calibrate = vertical_grados_calibrate };
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
    public async Task<cResultAnt> Start_laser(int pIsRead, int pLaser)
    {
        cResultAnt result = null;
        try
        {
            ProcessEsp32 oProcess = GetResource();
            if (oProcess != null)
            {
                result = await oProcess.actionAnt_laser(pIsRead, pLaser);
                SetResource(oProcess);
            }
            else
            {
                result = new cResultAnt();
                result.msg = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public async Task<cResultAnt> Start_servoAngle(double pHorizontal, double pVertical)
    {
        cResultAnt result = null;
        try
        {
            ProcessEsp32 oProcess = GetResource();
            if (oProcess != null)
            {
                result = await oProcess.actionAnt_servoAngle(pHorizontal, pVertical);
                SetResource(oProcess);
            }
            else
            {
                result = new cResultAnt();
                result.msg = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
    }
    public cResultAnt Start_star(Star pStar, bool isLaserOn = false)
    {
        cResultAnt result = new cResultAnt();
        try
        {
            ProcessEsp32 oProcess = GetResource();
            if (oProcess != null)
            {
                result = oProcess.actionAnt_star(pStar, isLaserOn);
                SetResource(oProcess);
            }
            else
            {
                //result.type = "Recurso no disponible o en uso";
                result.msg = "Recurso no disponible o en uso";
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
        return result;
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
    public async Task<cResultAnt> actionAnt_laser(int pIsRead, int pLaser)
    {
        cResultAnt result = null;
        Guid oAstroTracking = Util.newAstroTracking_laser(Constantes.astro_type_laser, pLaser);
        result = await getAstroTracking_ResultAnt(Constantes.astro_type_laser, oAstroTracking);
        if (result == null)
        {
            result = new cResultAnt();
            result.msg = "No se obtuvo respuesta";
        }
        return result;
    }
    public async Task<cResultAnt> actionAnt_servoAngle(double pHorizontal, double pVertical)
    {
        cResultAnt result = null;
        Guid oAstroTracking = Util.newAstroTracking(Constantes.astro_type_servoAngle, pHorizontal, pVertical);
        result = getAstroTracking_ResultAnt(Constantes.astro_type_servoAngle, oAstroTracking).Result;
        return result;
    }
    public async Task<cResultAnt> actionAnt_servoAngle_calibrate(double pHorizontal, double pVertical, double pH_calibrate, double pV_calibrate)
    {
        cResultAnt result = null;
        Guid oAstroTracking = Util.newAstroTracking(Constantes.astro_type_servoAngle_calibrate, pHorizontal, pVertical, pH_calibrate, pV_calibrate);
        result = getAstroTracking_ResultAnt(Constantes.astro_type_servoAngle_calibrate, oAstroTracking).Result;
        return result;
    }
    public cResultAnt actionAnt_star(Star pStar, bool isLaserOn = false)
    {
        cResultAnt result = null;
        if (pStar != null)
        {
            Guid oAstroTracking = Util.newAstroTracking(Constantes.astro_type_star, pStar.ra, pStar.dec);
            result = getAstroTracking_ResultAnt(Constantes.astro_type_star, oAstroTracking).Result;
            if (result != null)
            {
                ServoCoordinates oServoCoordinates = ServoCoordinates.convertServoCoordinates(result.hc);
                if (oServoCoordinates != null)
                {
                   //result.sc = oServoCoordinates;
                    //string strEq = "AR/Dec: " + AstronomyEngine.GetHHmmss(pStar.ra) + "/" + AstronomyEngine.GetSexagesimal(pStar.dec);
                    //string strHc = "Az./Alt.: " + AstronomyEngine.GetSexagesimal(result.hc.Azimuth) + "/" + AstronomyEngine.GetSexagesimal(result.hc.Altitude);
                    // result += strEq + "<br/>" + strHc + "<br/>";
                    // result += "HIP " + pStar.hip.ToString() + "<br/>";
                    result.hip = pStar.hip;
                    result.ec = new EquatorialCoordinates() { ra = pStar.ra, dec = pStar.dec };
                    result.hc = new HorizontalCoordinates() { Azimuth = result.hc.Azimuth, Altitude = result.hc.Altitude };
                }
                else
                {
                    result.msg = "Estrella no es visible";
                }
            }
            else
            {
                result = new cResultAnt();
                result.msg = "No se obtuvo respuesta";
            }
        }
        else
        {
            result = new cResultAnt();
            result.msg = "No se encontro estrella";
        }
        return result;
    }
    public async Task<cResultAnt> getAstroTracking_ResultAnt(string pType, Guid pGuid)
    {
        cResultAnt result = null;
        HorizontalCoordinates oHorizontalCoordinates = null;
        ServoCoordinates oServoCoordinates = null;
        string msg = string.Empty;
        int contador = 0;
        bool isFoundAntTracking = false;
        string status = Constantes.astro_status_movedServo;
        if (pType == Constantes.astro_type_laser)
        {
            status = Constantes.astro_status_movedLaser;
        }

        while (contador < 720)
        {
            using (var context = new AstroDbContext())
            {
                AntTracking oAntTracking = context.AntTrackings.Where(x => x.publicID == pGuid && x.status == status).FirstOrDefault();
                if (oAntTracking != null)
                {
                    result = new cResultAnt();
                    isFoundAntTracking = true;
                    if (pType == Constantes.astro_type_star)
                    {
                        oHorizontalCoordinates = new HorizontalCoordinates() { Altitude = oAntTracking.altitude.Value, Azimuth = oAntTracking.azimuth.Value };
                        oServoCoordinates = new ServoCoordinates() { servoH = oAntTracking.get_h_calibrate(), servoV = oAntTracking.get_v_calibrate(), _h_calibrate = oAntTracking._h_calibrate, _v_calibrate = oAntTracking._v_calibrate };
                    }
                    else if (pType == Constantes.astro_type_servoAngle || pType == Constantes.astro_type_servoAngle_calibrate)
                    {
                        oServoCoordinates = new ServoCoordinates() { servoH = oAntTracking.get_h_calibrate(), servoV = oAntTracking.get_v_calibrate(), _h_calibrate = oAntTracking._h_calibrate, _v_calibrate = oAntTracking._v_calibrate };
                        //oHorizontalCoordinates = new HorizontalCoordinates() { Altitude = oAntTracking.get_h_calibrate(), Azimuth = oAntTracking.get_v_calibrate() };
                    }
                    else if (pType == Constantes.astro_type_laser)
                    {
                        //oHorizontalCoordinates = new HorizontalCoordinates() { Altitude = 0, Azimuth = 0 };
                        msg = "laser: " + oAntTracking.isLaser;
                    }
                    break;
                }
            }
            await Task.Delay(50);
            contador++;
        }
        if (result != null)
        {
            if (oHorizontalCoordinates != null)
            {
                result.hc = oHorizontalCoordinates;
            }
            if (oServoCoordinates != null)
            {
                result.sc = oServoCoordinates;
            }
            if (!string.IsNullOrEmpty(msg))
            {
                result.msg = msg;
            }
        }
        if (!isFoundAntTracking)
        {
            await ProcessAntV2.AntTrackingStatus(pGuid, Constantes.astro_status_noResponseEsp32, null);
        }
        return result;
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