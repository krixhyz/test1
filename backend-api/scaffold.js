const fs = require('fs');
const path = require('path');

const root = __dirname;
const dirs = [
    'Domain/Entities', 'Domain/Enums',
    'Application/DTOs', 'Application/Interfaces', 'Application/Services', 'Application/Common',
    'Infrastructure/Data', 'Infrastructure/Configurations',
    'Controllers'
];

dirs.forEach(d => fs.mkdirSync(path.join(root, d), { recursive: true }));

const write = (filepath, content) => fs.writeFileSync(path.join(root, filepath), content.trim());

// --- DOMAIN ENTITIES ---
write('Domain/Entities/ApplicationUser.cs', `
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace WeatherAPI.Domain.Entities;
public class ApplicationUser : IdentityUser<Guid> {
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}`);

write('Domain/Entities/Customer.cs', `
namespace WeatherAPI.Domain.Entities;
public class Customer {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public ICollection<CustomerVehicle> Vehicles { get; set; } = new List<CustomerVehicle>();
}`);

write('Domain/Entities/CustomerVehicle.cs', `
namespace WeatherAPI.Domain.Entities;
public class CustomerVehicle {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string VehicleNumber { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
}`);

write('Domain/Entities/Vendor.cs', `
namespace WeatherAPI.Domain.Entities;
public class Vendor {
    public Guid Id { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}`);

write('Domain/Entities/Part.cs', `
namespace WeatherAPI.Domain.Entities;
public class Part {
    public Guid Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string PartCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
}`);

write('Domain/Entities/PurchaseInvoice.cs', `
namespace WeatherAPI.Domain.Entities;
public class PurchaseInvoice {
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}`);

write('Domain/Entities/PurchaseInvoiceItem.cs', `
namespace WeatherAPI.Domain.Entities;
public class PurchaseInvoiceItem {
    public Guid Id { get; set; }
    public Guid PurchaseInvoiceId { get; set; }
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public Guid PartId { get; set; }
    public Part Part { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal => Quantity * UnitCost;
}`);

write('Domain/Entities/SalesInvoice.cs', `
namespace WeatherAPI.Domain.Entities;
public class SalesInvoice {
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid StaffId { get; set; }
    public ApplicationUser Staff { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime? CreditDueDate { get; set; }
    public bool EmailSent { get; set; }
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
}`);

write('Domain/Entities/SalesInvoiceItem.cs', `
namespace WeatherAPI.Domain.Entities;
public class SalesInvoiceItem {
    public Guid Id { get; set; }
    public Guid SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;
    public Guid PartId { get; set; }
    public Part Part { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}`);

write('Domain/Entities/Appointment.cs', `
namespace WeatherAPI.Domain.Entities;
public class Appointment {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid VehicleId { get; set; }
    public CustomerVehicle Vehicle { get; set; } = null!;
    public DateTime AppointmentDate { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}`);

write('Domain/Entities/Review.cs', `
namespace WeatherAPI.Domain.Entities;
public class Review {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
}`);

write('Domain/Entities/Notification.cs', `
namespace WeatherAPI.Domain.Entities;
public class Notification {
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}`);

write('Domain/Entities/EmailLog.cs', `
namespace WeatherAPI.Domain.Entities;
public class EmailLog {
    public Guid Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}`);

write('Domain/Entities/UnavailablePartRequest.cs', `
namespace WeatherAPI.Domain.Entities;
public class UnavailablePartRequest {
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime Date { get; set; } = DateTime.UtcNow;
}`);

// --- DATA CONTEXT ---
write('Infrastructure/Data/ApplicationDbContext.cs', `
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
    }
}`);

// --- DTOs (Common ApiResponse & specific) ---
write('Application/Common/ApiResponse.cs', `
namespace WeatherAPI.Application.Common;
public class ApiResponse<T> {
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    
    public static ApiResponse<T> Ok(T data, string msg = "Success") => new() { Success = true, Message = msg, Data = data };
    public static ApiResponse<T> Fail(string msg, List<string>? errs = null) => new() { Success = false, Message = msg, Errors = errs };
}`);

write('Application/DTOs/AuthDtos.cs', `
using System.ComponentModel.DataAnnotations;
namespace WeatherAPI.Application.DTOs;

public class LoginDto {
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class RegisterCustomerDto {
    [Required] public string FullName { get; set; } = string.Empty;
    [Required] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    [Required] public string VehicleNumber { get; set; } = string.Empty;
    public string VehicleBrand { get; set; } = string.Empty;
    public string VehicleModel { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
}
`);

write('Application/DTOs/StaffDtos.cs', `
namespace WeatherAPI.Application.DTOs;
public class CreateStaffDto {
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
`);

write('Application/DTOs/PartDtos.cs', `
namespace WeatherAPI.Application.DTOs;
public class CreatePartDto {
    public string PartName { get; set; } = string.Empty;
    public string PartCode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int ReorderLevel { get; set; }
    public Guid VendorId { get; set; }
}
`);

write('Application/DTOs/SalesDtos.cs', `
namespace WeatherAPI.Application.DTOs;
public class CreateSalesInvoiceDto {
    public Guid CustomerId { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
    public DateTime? CreditDueDate { get; set; }
    public List<SalesItemDto> Items { get; set; } = new();
}
public class SalesItemDto {
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
}
`);

write('Application/DTOs/PurchaseDtos.cs', `
namespace WeatherAPI.Application.DTOs;
public class CreatePurchaseInvoiceDto {
    public Guid VendorId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<PurchaseItemDto> Items { get; set; } = new();
}
public class PurchaseItemDto {
    public Guid PartId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
}
`);

write('Application/DTOs/ReportsDto.cs', `
namespace WeatherAPI.Application.DTOs;
public class FinancialReportDto {
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal DiscountGiven { get; set; }
    public decimal CreditSales { get; set; }
    public decimal NetRevenue => TotalSales - DiscountGiven - TotalPurchases;
}
`);


// --- SERVICES ---
write('Application/Interfaces/IAuthService.cs', `
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;
namespace WeatherAPI.Application.Interfaces;
public interface IAuthService {
    Task<ApiResponse<object>> LoginAsync(LoginDto dto);
    Task<ApiResponse<object>> RegisterCustomerAsync(RegisterCustomerDto dto);
    Task SeedRolesAndAdminAsync();
}`);

write('Application/Services/AuthService.cs', `
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Infrastructure.Data;

namespace WeatherAPI.Application.Services;

public class AuthService : IAuthService {
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _db;

    public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, IConfiguration config, ApplicationDbContext db) {
        _userManager = userManager;
        _roleManager = roleManager;
        _config = config;
        _db = db;
    }

    public async Task<ApiResponse<object>> LoginAsync(LoginDto dto) {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return ApiResponse<object>.Fail("Invalid credentials");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Customer";

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "super_secret_key_1234567890123456"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return ApiResponse<object>.Ok(new {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = role,
            FullName = user.FullName,
            UserId = user.Id
        });
    }

    public async Task<ApiResponse<object>> RegisterCustomerAsync(RegisterCustomerDto dto) {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName, PhoneNumber = dto.Phone };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return ApiResponse<object>.Fail(result.Errors.First().Description);
        
        await _userManager.AddToRoleAsync(user, "Customer");

        var customer = new Customer { UserId = user.Id, Address = dto.Address };
        customer.Vehicles.Add(new CustomerVehicle { VehicleNumber = dto.VehicleNumber, Brand = dto.VehicleBrand, Model = dto.VehicleModel, VehicleType = dto.VehicleType });
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return ApiResponse<object>.Ok(new { UserId = user.Id });
    }

    public async Task SeedRolesAndAdminAsync() {
        string[] roles = { "Admin", "Staff", "Customer" };
        foreach (var r in roles) {
            if (!await _roleManager.RoleExistsAsync(r))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(r));
        }

        if (await _userManager.FindByEmailAsync("admin@vparts.com") == null) {
            var admin = new ApplicationUser { UserName = "admin@vparts.com", Email = "admin@vparts.com", FullName = "System Admin" };
            await _userManager.CreateAsync(admin, "Admin@123");
            await _userManager.AddToRoleAsync(admin, "Admin");
        }
        if (await _userManager.FindByEmailAsync("staff@vparts.com") == null) {
            var staff = new ApplicationUser { UserName = "staff@vparts.com", Email = "staff@vparts.com", FullName = "Demo Staff" };
            await _userManager.CreateAsync(staff, "Staff@123");
            await _userManager.AddToRoleAsync(staff, "Staff");
        }
    }
}`);

// --- BUSINESS SERVICES (Stock & Sales & Email) ---
write('Application/Services/InventoryService.cs', `
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Domain.Entities;
namespace WeatherAPI.Application.Services;
public class InventoryService {
    private readonly ApplicationDbContext _db;
    public InventoryService(ApplicationDbContext db) => _db = db;

    public async Task CheckLowStockAsync(Guid partId) {
        var part = await _db.Parts.FindAsync(partId);
        if (part != null && part.StockQuantity <= part.ReorderLevel) {
            _db.Notifications.Add(new Notification {
                Title = "Low Stock Alert",
                Message = $"Part {part.PartName} is running low ({part.StockQuantity} left).",
                Type = "Alert"
            });
            await _db.SaveChangesAsync();
        }
    }
}`);

write('Application/Services/SalesService.cs', `
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;
namespace WeatherAPI.Application.Services;
public class SalesService {
    private readonly ApplicationDbContext _db;
    private readonly InventoryService _inv;

    public SalesService(ApplicationDbContext db, InventoryService inv) { _db = db; _inv = inv; }

    public async Task<ApiResponse<object>> CreateSalesInvoiceAsync(CreateSalesInvoiceDto dto, Guid staffId) {
        decimal subtotal = 0;
        var invoice = new SalesInvoice {
            InvoiceNumber = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            CustomerId = dto.CustomerId,
            StaffId = staffId,
            PaymentStatus = dto.PaymentStatus,
            CreditDueDate = dto.CreditDueDate
        };

        foreach (var item in dto.Items) {
            var part = await _db.Parts.FindAsync(item.PartId);
            if (part == null || part.StockQuantity < item.Quantity)
                return ApiResponse<object>.Fail($"Not enough stock for part {item.PartId}");

            part.StockQuantity -= item.Quantity; // Decrement stock
            var lineTotal = part.UnitPrice * item.Quantity;
            subtotal += lineTotal;
            invoice.Items.Add(new SalesInvoiceItem { PartId = part.Id, Quantity = item.Quantity, UnitPrice = part.UnitPrice });
        }

        invoice.Subtotal = subtotal;
        
        // Feature 21: Loyalty discount
        if (subtotal > 5000) invoice.Discount = subtotal * 0.10m;
        else invoice.Discount = 0;

        invoice.TotalAmount = subtotal - invoice.Discount;
        _db.SalesInvoices.Add(invoice);

        if (dto.PaymentStatus == "Credit") {
            var customer = await _db.Customers.FindAsync(dto.CustomerId);
            if (customer != null) customer.CreditBalance += invoice.TotalAmount;
        }

        await _db.SaveChangesAsync();

        foreach (var item in dto.Items) await _inv.CheckLowStockAsync(item.PartId);

        return ApiResponse<object>.Ok(invoice);
    }
}`);

write('Application/Services/AiPredictionService.cs', `
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Infrastructure.Data;
namespace WeatherAPI.Application.Services;

public class AiPredictionService {
    private readonly ApplicationDbContext _db;
    public AiPredictionService(ApplicationDbContext db) => _db = db;

    public async Task<List<string>> PredictFailureAsync(Guid customerId) {
        // Feature 24: AI Vehicle Part Failure Prediction (Rule-based)
        var predictions = new List<string>();
        var invoices = await _db.SalesInvoices.Include(x => x.Items).ThenInclude(x => x.Part)
            .Where(x => x.CustomerId == customerId && x.Date < DateTime.UtcNow.AddMonths(-12)).ToListAsync();
        
        var oldParts = invoices.SelectMany(x => x.Items).Select(x => x.Part.Category).Distinct();
        if (oldParts.Contains("Brakes")) predictions.Add("Brake pads were replaced over 12 months ago. Inspection recommended.");
        if (oldParts.Contains("Engine")) predictions.Add("Engine oil filters are overdue for a change.");
        
        return predictions;
    }
}`);

// --- CONTROLLERS ---
write('Controllers/AuthController.cs', `
using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Interfaces;
namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) => Ok(await _auth.LoginAsync(dto));

    [HttpPost("register-customer")]
    public async Task<IActionResult> Register(RegisterCustomerDto dto) => Ok(await _auth.RegisterCustomerAsync(dto));

    [HttpPost("seed")]
    public async Task<IActionResult> Seed() { await _auth.SeedRolesAndAdminAsync(); return Ok("Seeded"); }
}`);

write('Controllers/StaffController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Common;
using WeatherAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase {
    private readonly UserManager<ApplicationUser> _userManager;
    public StaffController(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    [HttpGet]
    public async Task<IActionResult> GetStaff() {
        var staff = await _userManager.GetUsersInRoleAsync("Staff");
        return Ok(ApiResponse<object>.Ok(staff));
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffDto dto) {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email, FullName = dto.FullName, PhoneNumber = dto.Phone };
        var res = await _userManager.CreateAsync(user, dto.Password);
        if (res.Succeeded) await _userManager.AddToRoleAsync(user, "Staff");
        return Ok(ApiResponse<object>.Ok(user));
    }
}`);

write('Controllers/VendorsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class VendorsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public VendorsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Vendors.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Vendor vendor) {
        _db.Vendors.Add(vendor);
        await _db.SaveChangesAsync();
        return Ok(ApiResponse<Vendor>.Ok(vendor));
    }
}`);

write('Controllers/PartsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.DTOs;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize]
public class PartsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public PartsController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Parts.Include(x => x.Vendor).ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreatePartDto dto) {
        var part = new Part { PartName = dto.PartName, PartCode = dto.PartCode, Category = dto.Category, UnitPrice = dto.UnitPrice, ReorderLevel = dto.ReorderLevel, VendorId = dto.VendorId };
        _db.Parts.Add(part);
        await _db.SaveChangesAsync();
        return Ok(part);
    }
    
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLowStock() => Ok(await _db.Parts.Where(p => p.StockQuantity <= p.ReorderLevel).ToListAsync());
}`);

write('Controllers/CustomersController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class CustomersController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public CustomersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Customers.Include(c => c.User).Include(c => c.Vehicles).ToListAsync());

    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword) {
        var res = await _db.Customers.Include(c => c.User).Include(c => c.Vehicles)
            .Where(c => c.User.FullName.Contains(keyword) || c.User.PhoneNumber.Contains(keyword) || c.Vehicles.Any(v => v.VehicleNumber.Contains(keyword)))
            .ToListAsync();
        return Ok(res);
    }
}`);

write('Controllers/SalesInvoicesController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Application.Services;
using WeatherAPI.Application.DTOs;
using System.Security.Claims;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Staff,Admin")]
public class SalesInvoicesController : ControllerBase {
    private readonly SalesService _sales;
    public SalesInvoicesController(SalesService sales) => _sales = sales;

    [HttpPost]
    public async Task<IActionResult> Create(CreateSalesInvoiceDto dto) {
        var staffIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(staffIdStr, out var staffId)) return Unauthorized();
        var res = await _sales.CreateSalesInvoiceAsync(dto, staffId);
        return Ok(res);
    }
}`);

write('Controllers/ReportsController.cs', `
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.DTOs;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class ReportsController : ControllerBase {
    private readonly ApplicationDbContext _db;
    public ReportsController(ApplicationDbContext db) => _db = db;

    [HttpGet("financial")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetFinancial() {
        var sales = await _db.SalesInvoices.ToListAsync();
        var purchases = await _db.PurchaseInvoices.ToListAsync();
        var dto = new FinancialReportDto {
            TotalSales = sales.Sum(s => s.TotalAmount + s.Discount),
            TotalPurchases = purchases.Sum(p => p.TotalAmount),
            DiscountGiven = sales.Sum(s => s.Discount),
            CreditSales = sales.Where(s => s.PaymentStatus == "Credit").Sum(s => s.TotalAmount)
        };
        return Ok(dto);
    }

    [HttpGet("customers/pending-credits")]
    public async Task<IActionResult> GetPendingCredits() {
        var overdue = await _db.SalesInvoices.Include(s => s.Customer).ThenInclude(c => c.User)
            .Where(s => s.PaymentStatus == "Credit" && s.CreditDueDate < DateTime.UtcNow)
            .ToListAsync();
        return Ok(overdue);
    }
}`);
