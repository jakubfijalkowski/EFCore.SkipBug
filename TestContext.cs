using Microsoft.EntityFrameworkCore;

namespace EFCore.SkipBug
{
    public class TestContext : DbContext
    {
        public DbSet<RootEntity> RootEntities => Set<RootEntity>();
        public DbSet<SubEntity> SubEntities => Set<SubEntity>();

        public TestContext(DbContextOptions<TestContext> options)
            : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("bug");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RootEntity>(e =>
            {
                e.HasKey(i => i.Id);

                e.HasMany(i => i.SubEntities)
                    .WithOne(ip => ip.Root)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SubEntity>(e =>
            {
                e.HasKey(ip => new { ip.RootId, ip.CollectMe });
            });
        }
    }
}
