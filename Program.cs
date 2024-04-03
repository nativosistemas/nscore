using nscore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication("Bearer").AddJwtBearer();
        builder.Services.AddSingleton<nscore.LedClient>();
        builder.Services.AddSingleton<nscore.ProcessAnt>();
        builder.Services.AddSingleton<nscore.ProcessAntV2>();
        nscore.Util.WebRootPath = builder.Environment.WebRootPath;
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseAuthorization();

        nscore.Helper.app = builder.Configuration.GetSection("appSettings")["app"];
        nscore.Helper.folder = builder.Configuration.GetSection("appSettings")["folder"];// System.IO.Directory.GetCurrentDirectory();
        nscore.Helper.sqllite = builder.Configuration.GetSection("appSettings")["sqllite"];

        //
        nscore.AstroDbContext.initDbContext();
        int inicioApp = nscore.Util.inicioApp().Result; //       LlamarFuncionAsincronica().Wait(); 
        //
        app.MapGet("/logs", () => { return nscore.Util.getLogs(); });
        app.MapGet("/on", (nscore.LedClient pLed) => { pLed.LedOn(); return "LedOn"; });
        app.MapGet("/off", (nscore.LedClient pLed) => { pLed.LedOff(); return "LedOff"; });
        app.MapGet("/servo", ((nscore.ProcessAnt pProcessAnt, int id) => { return pProcessAnt.findStar(id, true); }));
        app.MapGet("/getservos", ((nscore.ProcessAnt pProcessAnt) => { return pProcessAnt.getValoresServos(); }));
        app.MapGet("/servomover", ((nscore.ProcessAnt pProcessAnt, double pH, double pV, double pH_min, double pH_max, double pV_min, double pV_max, bool pOnLaser) => { return pProcessAnt.actionAnt_servo(pH, pV, pH_min, pH_max, pV_min, pV_max, pOnLaser); }));
        app.MapGet("/servoconstellations", ((nscore.ProcessAnt pProcessAnt, int id) => { return pProcessAnt.findConstellation(id); }));
        //app.MapGet("/laser", ((nscore.ProcessAnt pProcessAnt, int read, int on) => { return pProcessAnt.actionLaser(read, on); }));
        app.MapGet("/stars", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.getStars()); }));
        app.MapGet("/stars_stellarium", ((nscore.ProcessAntV2 pProcessAntV2) => { return Results.Json(pProcessAntV2.getStars()); }));
        app.MapGet("/allstars", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(nscore.Util.getAllStars()); }));
        app.MapGet("/constellations", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.getConstellations()); }));
        app.MapGet("/updateconstellation", ((nscore.ProcessAnt pProcessAnt, int id, int idHD, string name) => { return nscore.Util.updateConstelacion(id, idHD, name); }));
        app.MapGet("/citys", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.getCitys()); }));
        app.MapGet("/setcity", ((nscore.ProcessAnt pProcessAnt, int id, string name, double lat, double lon) => { return Results.Json(pProcessAnt.setCity(id, name, lat, lon)); }));
        app.MapGet("/getcity", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.city); }));
        app.MapGet("/astro", ((nscore.ProcessAnt pProcessAnt, double h, double v, int laser) => { return pProcessAnt.moveTheAnt(h, v, laser); }));
        app.MapGet("/simbad", () => { return nscore.Util.getAstronomicalObjects(); });
        app.MapGet("/falta", () => { return nscore.Util.getAstronomicalObjects_fileLoad(); });
        app.MapGet("/astrotracking", (() => { return Util.getAntTrackings(); }));
        app.MapGet("/restore", () => { return nscore.Util.restore(); });
        app.MapGet("/stellarium", async () => { return await nscore.Util.getInfoStellarium(); });
        app.MapGet("/test", async () => { return await nscore.Util.Astronomical_stellarium_copia(); });
        // llevar
        app.MapGet("/esp32", async (int led) => { return await nscore.Util.esp32_util(led); });
        //app.MapGet("/esp32_getAstro", async (nscore.ProcessAntV2 pProcessAntV2) => { return await pProcessAntV2.esp32_getAstro(); });
        app.MapGet("/getservos_v2", async (nscore.ProcessAntV2 pProcessAntV2) => { return await pProcessAntV2.getValoresServos(); });
        app.MapGet("/clean", async (nscore.ProcessAntV2 pProcessAntV2) => { return await pProcessAntV2.removeTable(); });
        app.MapGet("/setConfig", async (nscore.ProcessAntV2 pProcessAntV2, double latitude, double longitude, double horizontal_grados_min, double horizontal_grados_max, double vertical_grados_min, double vertical_grados_max) => { return await pProcessAntV2.setConfig(latitude, longitude, horizontal_grados_min, horizontal_grados_max, vertical_grados_min, vertical_grados_max); });
        app.MapGet("/getConfig", async (nscore.ProcessAntV2 pProcessAntV2) => { return await pProcessAntV2.getConfig(); });
        app.MapGet("/laser", async (nscore.ProcessAntV2 pProcessAntV2, int read, int on) => { return await pProcessAntV2.actionAnt_laser(read, on); });
        app.MapGet("/servo_v2", async (nscore.ProcessAntV2 pProcessAntV2, int id) => { return await pProcessAntV2.actionAnt_star(id); });
        app.MapGet("/esp32_setAstro", async (nscore.ProcessAntV2 pProcessAntV2, string publicID, string pSessionDevice_publicID) => { return await pProcessAntV2.esp32_setAstro(publicID, pSessionDevice_publicID); });
        app.MapGet("/servomover_v2", ((nscore.ProcessAntV2 pProcessAntV2, double pH, double pV) => { return pProcessAntV2.actionAnt_servo(pH, pV); }));
        //app.MapGet("/sessionDeviceAdd", async (nscore.ProcessAntV2 pProcessAntV2, string pDevice_publicID, string pDevice_name) => { return await pProcessAntV2.sessionDeviceAdd(pDevice_publicID, pDevice_name); });
        //app.MapGet("/isSessionDeviceOk", async (string pSessionDevice_publicID) => { return await nscore.Util.isSessionDeviceOk(pSessionDevice_publicID); });
        app.MapGet("/actionAnt_getAntTracking", async (nscore.ProcessAntV2 pProcessAntV2, string pDevice_publicID, string pSessionDevice_publicID) => { return await pProcessAntV2.actionAnt_getAntTracking(pDevice_publicID, pSessionDevice_publicID); });
        app.MapGet("/", (nscore.ProcessAnt pProcessAnt) =>
        {
            string pathPageWeb = Path.Combine(nscore.Util.WebRootPath, "estrellas_v2.html");// "index.html"
            var html = System.IO.File.ReadAllText(pathPageWeb);
            return Results.Content(html, "text/html");
            //return "Ok"; 
        });
        app.MapGet("/image/{strImage}", (string r, string n, string an, string al, string c, string re, HttpContext http, CancellationToken token) =>
        {
            http.Response.Headers.CacheControl = $"public,max-age={TimeSpan.FromHours(24).TotalSeconds}";
            return Results.Stream(stream => ResizeImageAsync(r, n, an, al, c, re, stream, token), "image/jpeg");
        });
        //
        app.MapGet("/secret", (System.Security.Claims.ClaimsPrincipal user) => $"Hello {user.Identity?.Name}. My secret").RequireAuthorization();
        app.MapGet("/token", async (string pName, string pPass) =>
{
    string jwt = await Util.Jwt_GenerateToken(pName, pPass, "+0mOx5gMexKmtjjr8hZxdAZSYzAy4aWtd/GArNygLIg=", "dotnet-user-jwts");

    return Results.Ok(new
    {
        AccessToken = jwt == null ? "!Ok" : jwt
    });
});
        /* app.MapPost("/token", async (string pName, string pPass) =>
         {
             string jwt = await Util.Jwt_GenerateToken(pName, pPass, "pJwt_Key ", "pJwt_Issuer");

             return Results.Ok(new
             {
                 AccessToken = jwt == null ? "!Ok" : jwt
             });
         });*/


        //
        app.Run();


        async Task ResizeImageAsync(string r, string n, string an, string al, string c, string re, Stream stream, CancellationToken token)
        {
            string ruta = r;
            string nombre = n;
            string strAncho = an;
            string strAlto = al;
            string strColor = string.Empty;
            string RutaCompleta = Path.Combine(nscore.Helper.extFolder, "archivos", ruta);
            string RutaCompletaNombreArchivo = Path.Combine(RutaCompleta, nombre);
            var strPath = RutaCompletaNombreArchivo;
            using var image = await Image.LoadAsync(strPath, token);
            int width = image.Width / 4;
            int height = image.Height / 4;
            image.Mutate(x => x.Resize(width, height));
            await image.SaveAsync(stream, JpegFormat.Instance, cancellationToken: token);
        }
    }
}