using Microsoft.EntityFrameworkCore;

public class DbCont : DbContext
{
    public DbCont(DbContextOptions<DbCont> options) : base(options)
    {
    }
         protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Docs_User>().HasNoKey();

        // Add other configurations, if needed

        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Chapter> Chapters { get; set; }
    public DbSet<Docs_User> Docs_Users { get; set; }
    public DbSet<User> Users { get; set; }
}