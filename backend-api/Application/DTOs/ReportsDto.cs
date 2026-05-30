namespace WeatherAPI.Application.DTOs;
using System.Text.Json.Serialization;

public class FinancialReportDto {
    [JsonPropertyName("period")]
    public string Period { get; set; } = string.Empty;

    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }

    [JsonPropertyName("totalSales")]
    public decimal TotalSales { get; set; }

    [JsonPropertyName("totalPurchaseCost")]
    public decimal TotalPurchaseCost { get; set; }

    [JsonPropertyName("totalPurchases")]
    public decimal TotalPurchases { get; set; }

    [JsonPropertyName("discountGiven")]
    public decimal DiscountGiven { get; set; }

    [JsonPropertyName("netProfit")]
    public decimal NetProfit { get; set; }

    [JsonPropertyName("netRevenue")]
    public decimal NetRevenue { get; set; }

    [JsonPropertyName("creditSales")]
    public decimal CreditSales { get; set; }

    [JsonPropertyName("invoiceCount")]
    public int InvoiceCount { get; set; }
}