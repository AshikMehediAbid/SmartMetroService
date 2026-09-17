using SmartMetroService.Application.Models;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Interfaces.IManagers;

public interface ITicketService
{
    Task GenerateQrTicketAsync(GenerateTicketRequestDto request);
    Task<RapidPassResponseDto> GetOrCreateUserRapidPass(string? userEmail);
    Task<List<TicketResponseDto>?> GetTicketsOfAUserByTicketStatus(string? userEmail, TicketStatus ticketStatus);
    Task<Ticket> GetTicketByIdAsync(Guid ticketId);
    Task<RapidPassResponseDto> RefreshRapidPass(string? userEmail);
}
