using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Application.DTOs;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Controllers;
[ApiController][Route("api/[controller]")]
public class AuthController : ControllerBase {
    private readonly IAuthService _auth;
    private readonly UserManager<ApplicationUser> _userManager;
    public AuthController(IAuthService auth, UserManager<ApplicationUser> userManager) {
        _auth = auth; _userManager = userManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto) => Ok(await _auth.LoginAsync(dto));

    [HttpPost("register-customer")]
    public async Task<IActionResult> Register(RegisterCustomerDto dto) => Ok(await _auth.RegisterCustomerAsync(dto));

    [HttpPost("seed")]
    public async Task<IActionResult> Seed() { await _auth.SeedRolesAndAdminAsync(); return Ok("Seeded"); }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me() {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new { user.Id, user.Email, user.FullName, user.PhoneNumber, user.IsActive, Role = roles.FirstOrDefault() });
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto) {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        return Ok(new { message = "Password changed successfully." });
    }
}