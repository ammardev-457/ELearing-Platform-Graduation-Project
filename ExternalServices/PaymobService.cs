using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ELProject.Domain.Models;

namespace ELProject.ExternalServices
{
    public class PaymobService : IPaymobGatewayService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public PaymobService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }


        public async Task<string> CreatePaymentIntentAsync(Order order, ApplicationUser student, string paymentMethod = "Card")
        {
            var secretKey = _configuration["Paymob:SecretKey"];
            var integrationId = int.Parse(_configuration[$"Paymob:Integrations:{paymentMethod}"]);

            var requestBody = new
            {
                amount = order.Amount,
                currency = "EGP",
                payment_methods = new[] { integrationId },
                special_reference = order.PaymentRefernce,
                billing_data = new
                {
                    first_name = student.UserName ?? "Student",
                    last_name = "User",
                    email = (string.IsNullOrEmpty(student.Email) || !student.Email.Contains("@"))
                        ? "dumyemail@example.com"
                        : student.Email,
                    phone_number = student.PhoneNumber ?? "+201010101010",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    city = "NA",
                    country = "NA",
                    state = "NA"
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {secretKey}");

            var response = await _httpClient.PostAsync(
                "https://accept.paymob.com/v1/intention/",
                content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseString);
            }

            using var doc = JsonDocument.Parse(responseString);

            // set order.paymob_order.id
            order.PaymobOrderId = doc.RootElement.GetProperty("intention_order_id").GetInt64();
            return doc.RootElement.GetProperty("client_secret").GetString() ?? string.Empty;
        }
        public async Task<string> CalculateHmac(JsonElement obj, string secret)
        {
            string Get(JsonElement root, string path)
            {
                try
                {
                    var parts = path.Split('.');

                    JsonElement current = root;

                    foreach (var p in parts)
                    {
                        if (!current.TryGetProperty(p, out current))
                            return "";
                    }

                    return current.ValueKind switch
                    {
                        JsonValueKind.String => current.GetString() ?? "",
                        JsonValueKind.Number => current.ToString(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        _ => ""
                    };
                }
                catch
                {
                    return "";
                }
            }

            var fields = new List<string>
            {
                "amount_cents",
                "created_at",
                "currency",
                "error_occured",
                "has_parent_transaction",
                "id", // obj.id
                "integration_id",
                "is_3d_secure",
                "is_auth",
                "is_capture",
                "is_refunded",
                "is_standalone_payment",
                "is_voided",
                "order.id",
                "owner",
                "pending",
                "source_data.pan",
                "source_data.sub_type",
                "source_data.type",
                "success"
            };

            var sb = new StringBuilder();

            foreach (var field in fields)
                sb.Append(Get(obj, field));

            var data = sb.ToString();

            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));

            return await Task.FromResult(Convert.ToHexStringLower(hash));
        }
    }
}