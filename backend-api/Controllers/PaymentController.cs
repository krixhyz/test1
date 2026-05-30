using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherAPI.Application.Common;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Data;

namespace WeatherAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(ApplicationDbContext db, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        // ================= eSewa Payments =================

        [HttpPost("esewa/initiate/{invoiceId}")]
        public async Task<IActionResult> InitiateEsewa(Guid invoiceId)
        {
            var invoice = await _db.SalesInvoices
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                return NotFound(ApiResponse<object>.Fail("Invoice not found."));
            }

            var productCode = _config["Payment:eSewa:MerchantId"] ?? "EPAYTEST";
            var secretKey = _config["Payment:eSewa:SecretKey"] ?? "8gBm/:&EnhH.1/q";
            var transactionUuid = $"{invoiceId}-{DateTime.UtcNow.Ticks}";
            var totalAmountString = invoice.TotalAmount.ToString("F2");

            // Format of message for eSewa signature: total_amount=100.00,transaction_uuid=1234,product_code=EPAYTEST
            var signatureMessage = $"total_amount={totalAmountString},transaction_uuid={transactionUuid},product_code={productCode}";
            var signature = GenerateEsewaSignature(secretKey, signatureMessage);

            var formData = new System.Collections.Generic.Dictionary<string, string>
            {
                { "amount", totalAmountString },
                { "tax_amount", "0.00" },
                { "total_amount", totalAmountString },
                { "transaction_uuid", transactionUuid },
                { "product_code", productCode },
                { "product_service_charge", "0.00" },
                { "product_delivery_charge", "0.00" },
                { "success_url", $"http://localhost:5033/api/payment/esewa/verify?invoiceId={invoiceId}" },
                { "failure_url", "http://localhost:5173/#/customer/history" },
                { "signed_field_names", "total_amount,transaction_uuid,product_code" },
                { "signature", signature }
            };

            var response = new
            {
                formUrl = "https://rc-epay.esewa.com.np/api/epay/main/v2/form",
                formData = formData
            };

            return Ok(ApiResponse<object>.Ok(response));
        }

        [HttpGet("esewa/verify")]
        public async Task<IActionResult> VerifyEsewaGet([FromQuery] string data, [FromQuery] Guid invoiceId)
        {
            try
            {
                var decodedJson = Encoding.UTF8.GetString(Convert.FromBase64String(data));
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var esewaResponse = JsonSerializer.Deserialize<EsewaVerificationResponse>(decodedJson, options);

                if (esewaResponse != null && esewaResponse.Status.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase))
                {
                    var invoice = await _db.SalesInvoices.FindAsync(invoiceId);
                    if (invoice != null)
                    {
                        invoice.PaymentStatus = "Paid";
                        await _db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback log or handle error
            }

            return Redirect("http://localhost:5173/#/customer/history");
        }

        [HttpPost("esewa/verify")]
        public async Task<IActionResult> VerifyEsewaPost([FromQuery] string data, [FromQuery] Guid invoiceId)
        {
            try
            {
                var decodedJson = Encoding.UTF8.GetString(Convert.FromBase64String(data));
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var esewaResponse = JsonSerializer.Deserialize<EsewaVerificationResponse>(decodedJson, options);

                if (esewaResponse != null && esewaResponse.Status.Equals("COMPLETE", StringComparison.OrdinalIgnoreCase))
                {
                    var invoice = await _db.SalesInvoices.FindAsync(invoiceId);
                    if (invoice != null)
                    {
                        invoice.PaymentStatus = "Paid";
                        await _db.SaveChangesAsync();
                        return Ok(ApiResponse<object>.Ok(null, "Payment verified successfully."));
                    }
                }
                return BadRequest(ApiResponse<object>.Fail("Payment status was not COMPLETE or invoice not found."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail($"Verification failed: {ex.Message}"));
            }
        }

        // ================= Khalti Payments =================

        [HttpPost("khalti/initiate/{invoiceId}")]
        public async Task<IActionResult> InitiateKhalti(Guid invoiceId)
        {
            var invoice = await _db.SalesInvoices
                .Include(i => i.Customer).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                return NotFound(ApiResponse<object>.Fail("Invoice not found."));
            }

            var secretKey = _config["Payment:Khalti:SecretKey"] ?? "ec000ab610f740c6805c88a5f1803d83";
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", secretKey);

            var payload = new
            {
                return_url = $"http://localhost:5033/api/payment/khalti/verify?invoiceId={invoiceId}",
                website_url = "http://localhost:5173/",
                amount = (int)(invoice.TotalAmount * 100), // in Paisa
                purchase_order_id = invoice.Id.ToString(),
                purchase_order_name = $"Invoice {invoice.InvoiceNumber}",
                customer_info = new
                {
                    name = invoice.Customer?.User?.FullName ?? "Customer",
                    email = invoice.Customer?.User?.Email ?? "customer@example.com",
                    phone = invoice.Customer?.User?.PhoneNumber ?? "9800000000"
                }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://a.khalti.com/api/v2/epayment/initiate/", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return BadRequest(ApiResponse<object>.Fail($"Khalti initiation failed: {errorMsg}"));
            }

            var resStream = await response.Content.ReadAsStreamAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var khaltiResult = await JsonSerializer.DeserializeAsync<KhaltiInitiateResult>(resStream, options);

            return Ok(ApiResponse<object>.Ok(new
            {
                paymentUrl = khaltiResult?.Payment_Url,
                pidx = khaltiResult?.Pidx
            }));
        }

        [HttpGet("khalti/verify")]
        public async Task<IActionResult> VerifyKhaltiGet([FromQuery] string pidx, [FromQuery] Guid invoiceId)
        {
            try
            {
                var secretKey = _config["Payment:Khalti:SecretKey"] ?? "ec000ab610f740c6805c88a5f1803d83";
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", secretKey);

                var payload = new { pidx = pidx };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://a.khalti.com/api/v2/epayment/lookup/", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var resStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var lookupResult = await JsonSerializer.DeserializeAsync<KhaltiLookupResult>(resStream, options);

                    if (lookupResult != null && lookupResult.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        var invoice = await _db.SalesInvoices.FindAsync(invoiceId);
                        if (invoice != null)
                        {
                            invoice.PaymentStatus = "Paid";
                            await _db.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback log or handle error
            }

            return Redirect("http://localhost:5173/#/customer/history");
        }

        [HttpPost("khalti/verify")]
        public async Task<IActionResult> VerifyKhaltiPost([FromQuery] string pidx, [FromQuery] Guid invoiceId)
        {
            try
            {
                var secretKey = _config["Payment:Khalti:SecretKey"] ?? "ec000ab610f740c6805c88a5f1803d83";
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", secretKey);

                var payload = new { pidx = pidx };
                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://a.khalti.com/api/v2/epayment/lookup/", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var resStream = await response.Content.ReadAsStreamAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var lookupResult = await JsonSerializer.DeserializeAsync<KhaltiLookupResult>(resStream, options);

                    if (lookupResult != null && lookupResult.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        var invoice = await _db.SalesInvoices.FindAsync(invoiceId);
                        if (invoice != null)
                        {
                            invoice.PaymentStatus = "Paid";
                            await _db.SaveChangesAsync();
                            return Ok(ApiResponse<object>.Ok(null, "Payment verified successfully."));
                        }
                    }
                }
                return BadRequest(ApiResponse<object>.Fail("Payment status was not Completed or lookup failed."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail($"Verification failed: {ex.Message}"));
            }
        }

        // ================= Signature & Helper Methods =================

        private string GenerateEsewaSignature(string secretKey, string message)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                var hashMessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashMessage);
            }
        }

        private class EsewaVerificationResponse
        {
            public string Transaction_Code { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string Total_Amount { get; set; } = string.Empty;
            public string Transaction_Uuid { get; set; } = string.Empty;
            public string Product_Code { get; set; } = string.Empty;
            public string Signature { get; set; } = string.Empty;
        }

        private class KhaltiInitiateResult
        {
            public string Pidx { get; set; } = string.Empty;
            public string Payment_Url { get; set; } = string.Empty;
        }

        private class KhaltiLookupResult
        {
            public string Pidx { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public int Total_Amount { get; set; }
            public string Transaction_Id { get; set; } = string.Empty;
        }
    }
}
