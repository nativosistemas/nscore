using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // builder.Services.AddSingleton<nscore.IServoController, nscore.ServoController>();
        builder.Services.AddSingleton<nscore.LedClient>();
        //  builder.Services.AddSingleton<nscore.ServoClient>();
        builder.Services.AddSingleton<nscore.ProcessAnt>();
        nscore.Util.WebRootPath = builder.Environment.WebRootPath;
        var app = builder.Build();
        app.UseStaticFiles();

        nscore.Helper.app = builder.Configuration.GetSection("appSettings")["app"];
        nscore.Helper.folder = builder.Configuration.GetSection("appSettings")["folder"];// System.IO.Directory.GetCurrentDirectory();
        nscore.Helper.sqllite = builder.Configuration.GetSection("appSettings")["sqllite"];

        //
        nscore.AstroDbContext.initDbContext();
        nscore.AstroDbContext.initTableStar();
        //
        app.MapGet("/logs", () => { return nscore.Util.getLogs(); });
        app.MapGet("/on", (nscore.LedClient pLed) => { pLed.LedOn(); return "LedOn"; });
        app.MapGet("/off", (nscore.LedClient pLed) => { pLed.LedOff(); return "LedOff"; });
        app.MapGet("/servo", ((nscore.ProcessAnt pProcessAnt, int id) => { return pProcessAnt.findStar(id); }));
        app.MapGet("/laser", ((nscore.ProcessAnt pProcessAnt, int read, int on) => { return pProcessAnt.actionLaser(read, on); }));
        app.MapGet("/stars", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.getStars()); }));
        app.MapGet("/citys", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.getCitys()); }));
        app.MapGet("/setcity", ((nscore.ProcessAnt pProcessAnt, int id, string name, double lat, double lon) => { return Results.Json(pProcessAnt.setCity(id,name, lat, lon)); }));
        app.MapGet("/getcity", ((nscore.ProcessAnt pProcessAnt) => { return Results.Json(pProcessAnt.city);}));
        app.MapGet("/astro", ((nscore.ProcessAnt pProcessAnt, double h, double v, int laser) => { return pProcessAnt.moveTheAnt(h, v, laser); }));
        app.MapGet("/simbad", () => { return nscore.Util.getAstronomicalObjects(); });
        app.MapGet("/falta", () => { return nscore.Util.getAstronomicalObjects_fileLoad(); });
        app.MapGet("/", (nscore.ProcessAnt pProcessAnt) => { return pProcessAnt.findStar(4); });//pProcessAnt.findStar(4);
        app.MapGet("/image/{strImage}", (string r, string n, string an, string al, string c, string re, HttpContext http, CancellationToken token) =>
        {
            http.Response.Headers.CacheControl = $"public,max-age={TimeSpan.FromHours(24).TotalSeconds}";
            return Results.Stream(stream => ResizeImageAsync(r, n, an, al, c, re, stream, token), "image/jpeg");
        });

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