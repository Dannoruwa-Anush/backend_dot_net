using WebApplication1.Models;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.IService.Helper
{
    public interface IOrderFinancialService
    {
        Task ProcessPaymentRefundAsync(CustomerOrder order, DateTime now);
        Task ApplyOrderPaymentStatusUpdateAsync(int orderId, OrderPaymentStatusEnum newStatus);
    }
}