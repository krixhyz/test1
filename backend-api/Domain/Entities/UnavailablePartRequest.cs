namespace WeatherAPI.Domain.Entities;
public class UnavailablePartRequest {
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid? VehicleId { get; set; }
    public CustomerVehicle? Vehicle { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Urgency { get; set; } = "Medium";
    public string Status { get; set; } = "Pending";
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
}