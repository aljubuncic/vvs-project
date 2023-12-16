using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Threading.Tasks;
using System.Threading;
using Burgija.Models;
using Microsoft.AspNetCore.Identity;

namespace Burgija.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ToolType> ToolTypes { get; set; }
        DbSet<Store> Stores { get; set; }
        DbSet<Location> Locations { get; set; }
        DbSet<Tool> Tools { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<IdentityUser<int>> Users { get; set; }

        DbSet<Rent> Rents { get; set; }

        public DbSet<Administrator> Administrator { get; set; }
        public DbSet<Courier> Courier { get; set; }
        public DbSet<Delivery> Delivery { get; set; }
        public DbSet<Discount> Discount { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<RegisteredUser> RegisteredUser { get; set; }
        public DbSet<Rent> Rent { get; set; }
        public DbSet<Review> Review { get; set; }
        public DbSet<Store> Store { get; set; }
        public DbSet<Tool> Tool { get; set; }
        public DbSet<ToolType> ToolType { get; set; }

        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class;
        
    }
}