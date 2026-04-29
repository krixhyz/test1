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