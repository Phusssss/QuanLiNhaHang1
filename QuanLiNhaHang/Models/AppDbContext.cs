using Microsoft.EntityFrameworkCore;
using QuanLiNhaHang.Models;

namespace QuanLiNhaHang.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Table> Tables { get; set; }
        public DbSet<LoaiBan> LoaiBans { get; set; }
        public DbSet<KhuVuc> KhuVucs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<TypeProduct> TypeProducts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Timekeeping> Timekeepings { get; set; }  // Thêm DbSet cho Timekeeping

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình cho enum JobType
            modelBuilder.Entity<Timekeeping>()
                .Property(t => t.TypeJob)
                .HasConversion<string>();  // Chuyển enum thành string trong database
        }
    }
}