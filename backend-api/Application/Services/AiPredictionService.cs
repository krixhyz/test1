using Microsoft.EntityFrameworkCore;
using WeatherAPI.Infrastructure.Data;
namespace WeatherAPI.Application.Services;

public class AiPredictionService {
    private readonly ApplicationDbContext _db;
    public AiPredictionService(ApplicationDbContext db) => _db = db;

    public async Task<List<string>> PredictFailureAsync(Guid customerId) {
        // Feature 24: AI Vehicle Part Failure Prediction (Rule-based)
        var predictions = new List<string>();
        var invoices = await _db.SalesInvoices.Include(x => x.Items).ThenInclude(x => x.Part)
            .Where(x => x.CustomerId == customerId && x.Date < DateTime.UtcNow.AddMonths(-12)).ToListAsync();
        
        var oldParts = invoices.SelectMany(x => x.Items).Select(x => x.Part.Category).Distinct();
        if (oldParts.Contains("Brakes")) predictions.Add("Brake pads were replaced over 12 months ago. Inspection recommended.");
        if (oldParts.Contains("Engine")) predictions.Add("Engine oil filters are overdue for a change.");
        
        return predictions;
    }
}