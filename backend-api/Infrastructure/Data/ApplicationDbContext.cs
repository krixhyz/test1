using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid> {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerVehicle> CustomerVehicles { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Part> Parts { get; set; }
    public DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }
    public DbSet<SalesInvoice> SalesInvoices { get; set; }
    public DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    public DbSet<UnavailablePartRequest> UnavailablePartRequests { get; set; }
    public DbSet<ServiceHistory> ServiceHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
        builder.Entity<Part>().Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Entity<PurchaseInvoice>().Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Entity<PurchaseInvoiceItem>().Property(p => p.UnitCost).HasColumnType("decimal(18,2)");
        builder.Entity<SalesInvoice>().Property(p => p.Subtotal).HasColumnType("decimal(18,2)");
        builder.Entity<SalesInvoice>().Property(p => p.Discount).HasColumnType("decimal(18,2)");
        builder.Entity<SalesInvoice>().Property(p => p.TotalAmount).HasColumnType("decimal(18,2)");
        builder.Entity<SalesInvoiceItem>().Property(p => p.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Entity<Customer>().Property(p => p.CreditBalance).HasColumnType("decimal(18,2)");
        builder.Entity<ServiceHistory>().Property(p => p.Cost).HasColumnType("decimal(18,2)");
    }
}