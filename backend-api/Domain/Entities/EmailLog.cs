namespace WeatherAPI.Domain.Entities;
public class EmailLog {
    public Guid Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}