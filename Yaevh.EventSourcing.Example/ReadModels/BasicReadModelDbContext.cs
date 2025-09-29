using Microsoft.EntityFrameworkCore;

namespace Yaevh.EventSourcing.Example.ReadModels;

internal class BasicReadModelDbContext : DbContext
{
    public BasicReadModelDbContext(DbContextOptions<BasicReadModelDbContext> options) : base(options) { }


    public DbSet<BasicAccountReadModel> ReadModels => Set<BasicAccountReadModel>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BasicAccountReadModel>(entity =>
        {
            entity.HasKey(e => e.AccountNumber);
            entity.Property(e => e.AccountNumber).IsRequired()
                .HasConversion(e => e.ToString(), db => new Example.Model.AccountNumber(db));
            entity.Property(e => e.OwnerName).IsRequired();
            entity.Property(e => e.Currency).IsRequired()
                .HasConversion(e => e.ToString(), db => new Example.Model.Currency(db));
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IsClosed).IsRequired();
        });
    }
}
