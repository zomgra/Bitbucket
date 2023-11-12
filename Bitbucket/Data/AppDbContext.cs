
using Bitbucket.Models;
using Microsoft.EntityFrameworkCore;

namespace Bitbucket.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Shipment> Shipments { get; set; }
    }
}