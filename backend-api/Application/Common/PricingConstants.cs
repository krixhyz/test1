namespace WeatherAPI.Application.Common;

public static class PricingConstants
{
    /// <summary>
    /// Loyalty discount threshold - customers qualify for loyalty discount if subtotal exceeds this amount
    /// </summary>
    public const decimal LoyaltyThreshold = 10000m; // Rs 10,000

    /// <summary>
    /// Loyalty discount rate - 10% discount for qualifying purchases
    /// </summary>
    public const decimal LoyaltyDiscountRate = 0.10m; // 10%
}
