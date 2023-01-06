using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
DKbase.Helper.getTipoApp = builder.Configuration.GetSection("appSettings")["getTipoApp"];
DKbase.Helper.getFolder = builder.Configuration.GetSection("appSettings")["getFolder"];


DKbase.Helper.getConnectionStringSQL = builder.Configuration.GetConnectionString("ConnectionSQL");


app.MapGet("/", () => "Hello World!");
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
    string RutaCompleta = Path.Combine(DKbase.Helper.getFolder, "archivos", ruta);
    string RutaCompletaNombreArchivo = Path.Combine(RutaCompleta, nombre);
    var strPath = RutaCompletaNombreArchivo;
    using var image = await Image.LoadAsync(strPath, token);
    int width = image.Width / 4;
    int height = image.Height / 4;
    image.Mutate(x => x.Resize(width, height));
    await image.SaveAsync(stream, JpegFormat.Instance, cancellationToken: token);
}