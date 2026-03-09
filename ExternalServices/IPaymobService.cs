using ELProject.Domain.DTOs;
using ELProject.Domain.Models;

namespace ELProject.ExternalServices
{
    public interface IPaymobGatewayService
    {
        public Task<string> CreatePaymentIntentAsync(Order order, ApplicationUser student, string paymentMethod = "Card");
    }
}