namespace WeatherAPI.Domain.Entities;

public class ServiceHistory {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid VehicleId { get; set; }
    public CustomerVehicle Vehicle { get; set; } = null!;
    public string ServiceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Technician { get; set; }
    public decimal? Cost { get; set; }
    public DateTime ServiceDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Completed";
}
