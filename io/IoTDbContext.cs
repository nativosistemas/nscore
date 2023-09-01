using Microsoft.EntityFrameworkCore;
namespace nscore;
public class IoTDbContext : DbContext
{
    public DbSet<nscore.DHT> DHT { get; set; }
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
            using (var context = new IoTDbContext())
            {
                context.Database.EnsureCreated(); // Crea la base de datos si no existe
            }
        }
        catch (Exception ex)
        {
            Util.log(ex);
        }
    }

    public static List<DHT> getDHT()
    {
        List<DHT> result = new List<DHT>();
        try
        {
            using (var context = new IoTDbContext())
            {
                result = context.DHT.ToList();
            }
        }
        catch (Exception ex)
        {
             Util.log(ex);
        }
        return result;
    }
}
