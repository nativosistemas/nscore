using nscore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey
            (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true
        };
    });
        // 1. Agregar política de CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin() //WithOrigins("https://nativosistemas.github.io")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });
        builder.Services.AddSingleton<nscore.LedClient>();
        //builder.Services.AddSingleton<nscore.ProcessAnt>();
        builder.Services.AddSingleton<nscore.ProcessAntV2>();
        nscore.Util.WebRootPath = builder.Environment.WebRootPath;
        var app = builder.Build();
        app.UseStaticFiles();
        app.UseAuthorization();
        app.UseCors("AllowAll");
        //app.UseHsts();
        //app.UseHttpsRedirection();

        nscore.Helper.app = builder.Configuration.GetSection("appSettings")["app"];
        nscore.Helper.folder = builder.Configuration.GetSection("appSettings")["folder"];// System.IO.Directory.GetCurrentDirectory();
        nscore.Helper.sqllite = builder.Configuration.GetSection("appSettings")["sqllite"];
        nscore.Helper.Jwt_Issuer = builder.Configuration["Jwt:Issuer"];
        nscore.Helper.Jwt_Audience = builder.Configuration["Jwt:Audience"];
        nscore.Helper.Jwt_Key = builder.Configuration["Jwt:Key"];
        nscore.Helper.user_name = builder.Configuration["user:name"];
        nscore.Helper.user_pass = builder.Configuration["user:pass"];
        nscore.Helper.IoT_esp32 = builder.Configuration["IoT:esp32"];
        nscore.Helper.IoT_esp32_stepper = builder.Configuration["IoT:esp32_stepper"];
        //
        nscore.AstroDbContext.initDbContext();
        int inicioApp = nscore.Util.inicioApp().Result; //       LlamarFuncionAsincronica().Wait(); 
                                                        //
      //  app.MapGet("/stellarium", async () => { return await nscore.Util.getInfoStellarium(); });
        // llevar
     //   app.MapGet("/stars_stellarium", ((nscore.ProcessAntV2 pProcessAntV2) => { return Results.Json(pProcessAntV2.getStars()); }));
      //  app.MapGet("/esp32", async (int led) => { return await nscore.Util.esp32_util(led); });
      //  app.MapGet("/getservos_v2", async (nscore.ProcessAntV2 pProcessAntV2) => { return await pProcessAntV2.getValoresServos(); });
       // app.MapGet("/setConfig_calibrate", async (nscore.ProcessAntV2 pProcessAntV2, double horizontal_grados_calibrate, double vertical_grados_calibrate) => { return await pProcessAntV2.setConfig_calibrate(horizontal_grados_calibrate, vertical_grados_calibrate); });

        //app.MapGet("/laser", async (nscore.ProcessAntV2 pProcessAntV2, int read, int on) => { return await pProcessAntV2.actionAnt_laser(on); });
       // app.MapGet("/servo_v2", async (nscore.ProcessAntV2 pProcessAntV2, int id) => { return await pProcessAntV2.actionAnt_star(id); });

        //app.MapGet("/servomover_v2", async (nscore.ProcessAntV2 pProcessAntV2, double pH, double pV) => { return pProcessAntV2.actionAnt_servo(pH, pV); });

       // app.MapGet("/tokenDevice", async (nscore.ProcessAntV2 pProcessAntV2, string pDevice_publicID) => { return await pProcessAntV2.sessionDeviceAdd(pDevice_publicID); });
        /* app.MapGet("/", () =>
         {
             string pathPageWeb = Path.Combine(nscore.Util.WebRootPath, "estrellas_v2.html");// "index.html"
             var html = System.IO.File.ReadAllText(pathPageWeb);
             return Results.Content(html, "text/html");
             //return Guid.NewGuid().ToString();
         });*/
        app.MapGet("/", () =>
  {
      return Results.Redirect("https://nativosistemas.github.io/seweb");
  });
  /*
        app.MapGet("/image/{strImage}", (string r, string n, string an, string al, string c, string re, HttpContext http, CancellationToken token) =>
        {
            http.Response.Headers.CacheControl = $"public,max-age={TimeSpan.FromHours(24).TotalSeconds}";
            return Results.Stream(stream => ResizeImageAsync(r, n, an, al, c, re, stream, token), "image/jpeg");
        });
        app.MapGet("/secret", async (System.Security.Claims.ClaimsPrincipal user) => $"Hello {user.FindFirst(JwtRegisteredClaimNames.Name).Value}. My secret").RequireAuthorization();
        app.MapPost("/token",
        [Microsoft.AspNetCore.Authorization.AllowAnonymous] async (request_User pUser) =>
        {
            string result = await Util.login(pUser);
            if (!string.IsNullOrEmpty(result))
            {
                return Results.Ok(result);
            }
            return Results.Unauthorized();
        });
        */
        ///
        /// 
        /// 
        /// api para react 
        app.MapPost("/login",
 [Microsoft.AspNetCore.Authorization.AllowAnonymous] async ([Microsoft.AspNetCore.Mvc.FromBody] request_User pUser) =>
 {
     string result = await Util.login(pUser);
     if (!string.IsNullOrEmpty(result))
     {
         return Results.Ok(result);
     }
     return Results.Unauthorized();
 });
        app.MapGet("/estrellas", async (System.Security.Claims.ClaimsPrincipal user, nscore.ProcessAntV2 pProcessAntV2) => { return Results.Json(pProcessAntV2.getStars()); }).RequireAuthorization();
        app.MapPost("/actionAnt", async (nscore.ProcessAntV2 pProcessAntV2, [Microsoft.AspNetCore.Mvc.FromBody] ActionAntRequest pValue) => { return await pProcessAntV2.actionAnt(pValue); }).RequireAuthorization();
        app.MapGet("/lastvalueservo", async (nscore.ProcessAntV2 pProcessAntV2) => { return await pProcessAntV2.getLastValuesServos(); }).RequireAuthorization();
        app.MapPost("/laser", async (nscore.ProcessAntV2 pProcessAntV2, [Microsoft.AspNetCore.Mvc.FromBody] ActionAntRequest pValue) => { return await pProcessAntV2.api_laser(pValue); }).RequireAuthorization();
        app.MapGet("/logs", () => { return nscore.Util.getLogs(); }).RequireAuthorization();
        app.MapGet("/getConfig", async (nscore.ProcessAntV2 pProcessAntV2) => { return await nscore.ProcessAntV2.getConfig(); }).RequireAuthorization();
        app.MapPost("/setConfig", async (nscore.ProcessAntV2 pProcessAntV2, [Microsoft.AspNetCore.Mvc.FromBody] ConfigAnt pValue) => { return await pProcessAntV2.setConfig(pValue); }).RequireAuthorization();
        app.MapGet("/astrotracking", (() => { return Util.getAntTrackings(); })).RequireAuthorization();
        app.MapGet("/resetear_baseDatos", () => { return nscore.Util.resetear_baseDatos(); }).RequireAuthorization();
        app.MapGet("/antResetZero", async (nscore.ProcessAntV2 pProcessAntV2) => { return pProcessAntV2.newAstroTrackingResetZero(); }).RequireAuthorization();

        // api esp32
        app.MapGet("/actionAnt_getAntTracking", async (nscore.ProcessAntV2 pProcessAntV2, string pDevice_publicID, string pSessionDevice_publicID) => { return await pProcessAntV2.actionAnt_getAntTracking(pDevice_publicID, pSessionDevice_publicID); });
        app.MapGet("/esp32_setAstro", async (nscore.ProcessAntV2 pProcessAntV2, string publicID, string pSessionDevice_publicID) => { return await pProcessAntV2.esp32_setAstro(publicID, pSessionDevice_publicID); });

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