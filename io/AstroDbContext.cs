using Microsoft.EntityFrameworkCore;
namespace nscore;
public class AstroDbContext : DbContext
{
    public DbSet<nscore.Star> Stars { get; set; }

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
            Console.WriteLine(ex.Message);
            //DKbase.generales.Log.LogError(System.Reflection.MethodBase.GetCurrentMethod(), ex, DateTime.Now);
        }
    }
    public static void initTableStar()
    {
        try
        {
            using (var context = new AstroDbContext())
            {
                int countStars = getStars().Count;
                if (countStars <= 0)
                {
                    var l = Util.getStars();
                    foreach (var item in l)
                    {
                        context.Stars.Add(item);
                    }
                    context.SaveChanges();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    public static List<Star> getStars()
    {
        List<Star> result = new List<Star>();
        try
        {
            using (var context = new AstroDbContext())
            {
                result = context.Stars.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return result;
    }
}
