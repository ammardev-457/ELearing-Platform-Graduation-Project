namespace ELProject.Services
{
    public interface IOrderService
    {
        Task<long> CreateOrderAsync(long userId, long courseId);
        Task MarkOrderAsPaidAsync(long orderId);
        Task MarkOrderAsFailedAsync(long orderId);
    }
}