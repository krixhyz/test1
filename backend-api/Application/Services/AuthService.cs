using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        Guid? customerId = null;
        if (role == "Customer") {
            customerId = await _db.Customers.AsNoTracking()
                .Where(c => c.UserId == user.Id)
                .Select(c => (Guid?)c.Id)
                .FirstOrDefaultAsync();
        }

        return ApiResponse<object>.Ok(new {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Role = role,
            FullName = user.FullName,
            UserId = user.Id,
            CustomerId = customerId
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

        return ApiResponse<object>.Ok(new { UserId = user.Id, CustomerId = customer.Id });
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
}
