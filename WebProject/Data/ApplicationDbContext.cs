using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuOption> MenuOptions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemOption> OrderItemOptions { get; set; }
        public DbSet<MenuItemRating> MenuItemRatings { get; set; }
        public DbSet<CaretakerRating> CaretakerRatings { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OrderItemOption>()
                .HasOne(o => o.OrderItem)
                .WithMany(i => i.SelectedOptions)
                .HasForeignKey(o => o.OrderItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItemOption>()
                .HasOne(o => o.MenuOption)
                .WithMany()
                .HasForeignKey(o => o.MenuOptionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(o => o.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<OrderItem>()
                .HasOne(o => o.MenuItem)
                .WithMany()
                .HasForeignKey(o => o.MenuItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<MenuOption>()
                .HasOne(o => o.MenuItem)
                .WithMany(m => m.MenuOptions)
                .HasForeignKey(o => o.MenuItemId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}