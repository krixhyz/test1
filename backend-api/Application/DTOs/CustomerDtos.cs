using System.ComponentModel.DataAnnotations;
namespace WeatherAPI.Application.DTOs;

public class CreateCustomerDto {
    [Required][MaxLength(150)] public string FullName { get; set; } = string.Empty;
    [Required][Phone] public string Phone { get; set; } = string.Empty;
    [EmailAddress] public string? Email { get; set; }
    [MaxLength(250)] public string? Address { get; set; }
}
public class UpdateCustomerDto {
    [Required][MaxLength(150)] public string FullName { get; set; } = string.Empty;
    [Required][Phone] public string Phone { get; set; } = string.Empty;
    [EmailAddress] public string? Email { get; set; }
    [MaxLength(250)] public string? Address { get; set; }
}
public class CreateVehicleDto {
    [Required][MaxLength(20)] public string VehicleNumber { get; set; } = string.Empty;
    [Required][MaxLength(50)] public string VehicleType { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Brand { get; set; } = string.Empty;
    [MaxLength(100)] public string? Model { get; set; }
}
public class UpdateVehicleDto {
    [Required][MaxLength(20)] public string VehicleNumber { get; set; } = string.Empty;
    [Required][MaxLength(50)] public string VehicleType { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Brand { get; set; } = string.Empty;
    [MaxLength(100)] public string? Model { get; set; }
}
public class ChangePasswordDto {
    [Required] public string CurrentPassword { get; set; } = string.Empty;
    [Required][MinLength(6)] public string NewPassword { get; set; } = string.Empty;
}
