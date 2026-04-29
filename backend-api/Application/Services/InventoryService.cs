using WeatherAPI.Infrastructure.Data;
using WeatherAPI.Domain.Entities;
namespace WeatherAPI.Application.Services;
public class InventoryService {
    private readonly ApplicationDbContext _db;
    public InventoryService(ApplicationDbContext db) => _db = db;

    public async Task CheckLowStockAsync(Guid partId) {
        var part = await _db.Parts.FindAsync(partId);
        if (part != null && part.StockQuantity <= part.ReorderLevel) {
            _db.Notifications.Add(new Notification {
                Title = "Low Stock Alert",
                Message = $"Part {part.PartName} is running low ({part.StockQuantity} left).",
                Type = "Alert"
            });
            await _db.SaveChangesAsync();
        }
    }
}