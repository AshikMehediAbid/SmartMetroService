using SmartMetroService.Application.Interfaces.IRepositories;
using Microsoft.EntityFrameworkCore;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public Task<Payment?> GetByMerTxnIdAsync(string merTxnId)
    {
        return _dbSet.SingleOrDefaultAsync(payment => payment.MerTxnId == merTxnId);
    }
}
