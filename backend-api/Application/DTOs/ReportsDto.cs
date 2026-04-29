namespace WeatherAPI.Application.DTOs;
public class FinancialReportDto {
    public decimal TotalSales { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal DiscountGiven { get; set; }
    public decimal CreditSales { get; set; }
    public decimal NetRevenue => TotalSales - DiscountGiven - TotalPurchases;
}