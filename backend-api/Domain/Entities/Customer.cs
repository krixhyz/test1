namespace WeatherAPI.Domain.Entities;
public class Customer {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public ICollection<CustomerVehicle> Vehicles { get; set; } = new List<CustomerVehicle>();
}