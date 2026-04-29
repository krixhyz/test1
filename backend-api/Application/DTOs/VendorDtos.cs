using System.ComponentModel.DataAnnotations;
namespace WeatherAPI.Application.DTOs;

public class CreateVendorDto {
    [Required][MaxLength(150)] public string VendorName { get; set; } = string.Empty;
    [Required][Phone] public string Phone { get; set; } = string.Empty;
    [EmailAddress] public string? Email { get; set; }
    [MaxLength(250)] public string? Address { get; set; }
    [MaxLength(100)] public string? ContactPerson { get; set; }
}
public class UpdateVendorDto {
    [Required][MaxLength(150)] public string VendorName { get; set; } = string.Empty;
    [Required][Phone] public string Phone { get; set; } = string.Empty;
    [EmailAddress] public string? Email { get; set; }
    [MaxLength(250)] public string? Address { get; set; }
    [MaxLength(100)] public string? ContactPerson { get; set; }
    public bool IsActive { get; set; } = true;
}
