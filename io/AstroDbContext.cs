using Microsoft.EntityFrameworkCore;
namespace nscore;
public class AstroDbContext : DbContext
{
    public DbSet<nscore.AstronomicalObject> AstronomicalObjects { get; set; }
    public DbSet<nscore.Constellation> Constellations { get; set; }
    public DbSet<nscore.AstroTracking> AstroTrackings { get; set; }
    public DbSet<nscore.AntTracking> AntTrackings { get; set; } 
    public DbSet<nscore.Config> Configs { get; set; }
    public DbSet<nscore.Log> Logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string strDataSource = Path.Combine(nscore.Helper.folder, nscore.Helper.sqllite);
        optionsBuilder.UseSqlite("Data Source=" + strDataSource);
    }
    public static void initDbContext()
    {
        try
        {
            using (var context = new AstroDbContext())
            {
                context.Database.EnsureCreated(); // Crea la base de datos si no existe
            }
        }
        catch (Exception ex)
        {
           // Console.WriteLine(ex.Message);
            Util.log(ex);
            //DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
    }
}

