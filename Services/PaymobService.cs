using System.Text;
using System.Text.Json;
using ELProject.Domain.Models;

namespace ELProject.Services
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
                    first_name = student.UserName?.Split(' ')[0] ?? "Student",
                    last_name = "User",
                    email = (string.IsNullOrEmpty(student.Email) || !student.Email.Contains("@"))
                        ? "customer@example.com"
                        : student.Email,
                    phone_number = student.PhoneNumber ?? "+201010101010",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    city = "Cairo",
                    country = "Egypt",
                    state = "Cairo"
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {secretKey}");

            var response = await _httpClient.PostAsync("https://accept.paymob.com/v1/intention/", content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Paymob Error: {responseString}");
                return string.Empty;
            }

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("client_secret").GetString() ?? string.Empty;
        }
    }
}