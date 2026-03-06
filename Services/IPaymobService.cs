using ELProject.Domain.DTOs;
using ELProject.Domain.Models;

namespace ELProject.Services
{
    public interface IPaymobGatewayService
    {
        public Task<string> CreatePaymentIntentAsync(Order order, ApplicationUser student, string paymentMethod = "Card");
    }
}