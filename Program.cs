using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<nscore.IServoController, nscore.ServoController>();
        var app = builder.Build();


        nscore.Helper.app = builder.Configuration.GetSection("appSettings")["app"];
        nscore.Helper.folder = System.IO.Directory.GetCurrentDirectory();// builder.Configuration.GetSection("appSettings")["folder"];

        DKbase.Helper.getFolder = nscore.Helper.folder;
        DKbase.Helper.getTipoApp = nscore.Helper.app;
        DKbase.Helper.getConnectionStringSQL = builder.Configuration.GetConnectionString("ConnectionSQL");


        app.MapGet("/", (nscore.IServoController pServo) => nscore.Util.MoverServo(pServo));
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