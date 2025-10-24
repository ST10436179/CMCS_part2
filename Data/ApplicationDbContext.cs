using System.Collections.Generic;
using System.Reflection.Emit;
using CMCSCopilot.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCSCopilot.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimFile> ClaimFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Claim>().HasMany(c => c.Files).WithOne(f => f.Claim).HasForeignKey(f => f.ClaimId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
