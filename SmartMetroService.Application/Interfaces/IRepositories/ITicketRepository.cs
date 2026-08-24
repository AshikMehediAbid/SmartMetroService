using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IRepositories;

public interface ITicketRepository : IRepository<Ticket>
{
    Task<byte[]> GetQrByteByIdAsync(Guid id);
    Task<Ticket?> GetTicketByIdAsync(Guid id);
    Task<List<Ticket>?> GetTicketsOfAUserByTicketStatusAsync(Guid id, TicketStatus ticketStatus);
    Task MarkOldFreshTicketsAsExpiredAsync(Guid id);
}
