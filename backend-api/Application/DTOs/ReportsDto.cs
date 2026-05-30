namespace WeatherAPI.Application.DTOs;
using System.Text.Json.Serialization;

public class FinancialReportDto {
    [JsonPropertyName("totalSales")]
    public decimal TotalSales { get; set; }

    [JsonPropertyName("totalPurchases")]
    public decimal TotalPurchases { get; set; }

    [JsonPropertyName("discountGiven")]
    public decimal DiscountGiven { get; set; }

    [JsonPropertyName("creditSales")]
    public decimal CreditSales { get; set; }

    [JsonPropertyName("netRevenue")]
    public decimal NetRevenue { get; set; }

    [JsonPropertyName("invoiceCount")]
    public int InvoiceCount { get; set; }
}