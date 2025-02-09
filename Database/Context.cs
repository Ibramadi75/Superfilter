using Database.Models;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users { get; set; }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<House> Houses { get; set; }
    public DbSet<City> Cities { get; set; } // Ajout de DbSet pour City

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).HasMaxLength(100);

            // Relation User -> House (1-1)
            entity.HasOne(u => u.House)
                .WithOne(h => h.User)
                .HasForeignKey<House>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Supprime la maison si l'utilisateur est supprimé

            // Relation User -> Car (1-1)
            entity.HasOne(u => u.Car)
                .WithOne(c => c.User) // Ajout de la navigation inverse dans Car
                .HasForeignKey<Car>(c => c.UserId) // Ajout d'une clé étrangère UserId dans Car
                .OnDelete(DeleteBehavior.SetNull); // Garde la voiture si l'utilisateur est supprimé
        });

        // Car Configuration
        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100);

            // Relation Car -> Brand (N-1)
            entity.HasOne(c => c.Brand)
                .WithMany(b => b.Cars) // Ajout de la navigation inverse dans Brand
                .HasForeignKey(c => c.BrandId) // Utilisation de la clé étrangère BrandId
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression d'une marque si une voiture l'utilise

            // Relation Car -> User (1-1)
            entity.HasOne(c => c.User)
                .WithOne(u => u.Car)
                .HasForeignKey<Car>(c => c.UserId)
                .OnDelete(DeleteBehavior.SetNull); // Garde la voiture si l'utilisateur est supprimé
        });

        // Brand Configuration
        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).HasMaxLength(100);

            // Relation inverse Brand -> Cars (1-N)
            entity.HasMany(b => b.Cars)
                .WithOne(c => c.Brand)
                .HasForeignKey(c => c.BrandId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression d'une marque si une voiture l'utilise
        });

        // House Configuration
        modelBuilder.Entity<House>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Address).HasMaxLength(200);

            // Relation House -> User (1-1)
            entity.HasOne(h => h.User)
                .WithOne(u => u.House)
                .HasForeignKey<House>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Supprime la maison si l'utilisateur est supprimé

            // Relation House -> City (N-1)
            entity.HasOne(h => h.City)
                .WithMany() // Pas de navigation inverse dans City pour l'instant
                .HasForeignKey(h => h.CityId)
                .OnDelete(DeleteBehavior.Restrict); // Empêche la suppression d'une ville si une maison l'utilise
        });

        // City Configuration
        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100); // Exemple de propriété supplémentaire
        });
    }
}