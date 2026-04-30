using System.ComponentModel.DataAnnotations;
namespace WeatherAPI.Application.DTOs;

public class CreateStaffDto {
    [Required][MaxLength(150)] public string FullName { get; set; } = string.Empty;
    [Required][EmailAddress] public string Email { get; set; } = string.Empty;
    [Required][MinLength(6)] public string Password { get; set; } = string.Empty;
    [MaxLength(100)] public string? Position { get; set; }
    [Phone] public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
public class UpdateStaffDto {
    [Required][MaxLength(150)] public string FullName { get; set; } = string.Empty;
    [MaxLength(100)] public string? Position { get; set; }
    [Phone] public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}
