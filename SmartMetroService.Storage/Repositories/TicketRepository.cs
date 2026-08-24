using Microsoft.EntityFrameworkCore;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;
using SmartMetroService.Storage.Sql;

namespace SmartMetroService.Storage.Repositories;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(MyApplicationDbContext db) : base(db)
    {
    }

    public async Task<byte[]> GetQrByteByIdAsync(Guid id)
    {
        var qrByte = await _dbSet
            .Where(t => t.Id == id)
            .Select(t => t.QRByte)
            .FirstOrDefaultAsync();

        return qrByte;
    }

    public async Task<Ticket?> GetTicketByIdAsync(Guid id)
    {
        return await _dbSet
            .Include(x => x.FromStation)
            .Include(x => x.ToStation)
            .FirstOrDefaultAsync(x => x.Id == id);
    }


    public async Task<List<Ticket>?> GetTicketsOfAUserByTicketStatusAsync(Guid id, TicketStatus ticketStatus)
    {
        var tickets = await _dbSet
            .Include(t => t.FromStation)
            .Include(t => t.ToStation)
            .Where(t => t.UserId == id && t.TicketStatus == ticketStatus)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();

        return tickets;
    }

    public async Task MarkOldFreshTicketsAsExpiredAsync(Guid id)
    {
        await _dbSet
            .Where(t => t.UserId == id &&
                        t.TicketStatus == TicketStatus.Fresh &&
                        t.ExpiryTime < DateTime.UtcNow)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.TicketStatus, TicketStatus.Expired));
    }
}
