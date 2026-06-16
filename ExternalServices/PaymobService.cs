using System.Text;
using System.Text.Json;
using ELProject.Domain.Models;

namespace ELProject.ExternalServices
{
    public class PaymobService
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
                special_reference = order.Id.ToString(),
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

            Console.WriteLine($"Status Code: {(int)response.StatusCode}");
            Console.WriteLine("Paymob Response:");
            Console.WriteLine(responseString);


            using var doc = JsonDocument.Parse(responseString);
            // set order.paymob_order.id
            order.PaymobOrderId = doc.RootElement.GetProperty("intention_order_id").GetInt64();
            return doc.RootElement.GetProperty("client_secret").GetString() ?? string.Empty;
        }
    }
}