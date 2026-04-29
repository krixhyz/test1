using System.ComponentModel.DataAnnotations;
namespace WeatherAPI.Application.DTOs;

public class CreatePartDto {
    [Required][MaxLength(150)] public string PartName { get; set; } = string.Empty;
    [Required][MaxLength(50)] public string PartCode { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Category { get; set; } = string.Empty;
    [Required][Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }
    [Range(0, int.MaxValue)] public int StockQuantity { get; set; }
    [Range(0, int.MaxValue)] public int ReorderLevel { get; set; } = 10;
    public Guid? VendorId { get; set; }
}
public class UpdatePartDto {
    [Required][MaxLength(150)] public string PartName { get; set; } = string.Empty;
    [Required][MaxLength(50)] public string PartCode { get; set; } = string.Empty;
    [Required][MaxLength(100)] public string Category { get; set; } = string.Empty;
    [Required][Range(0, double.MaxValue)] public decimal UnitPrice { get; set; }
    [Range(0, int.MaxValue)] public int ReorderLevel { get; set; } = 10;
    public Guid? VendorId { get; set; }
}