using KernelDatabase.Models;
using Microsoft.EntityFrameworkCore;

namespace KernelDatabase;

public class KernelDbContext : DbContext
{
    public virtual DbSet<FarmerUser> FarmerUsers { get; set; } = null!;
    public virtual DbSet<MessageChainLink> MessageChains { get; set; } = null!;

    public KernelDbContext(DbContextOptions<KernelDbContext> options) : base(options)
    {
    }
}