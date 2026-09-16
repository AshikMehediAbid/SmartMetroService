using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByMerTxnIdAsync(string merTxnId);
}
