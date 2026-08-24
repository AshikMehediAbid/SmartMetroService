namespace SmartMetroService.Application.Interfaces.IManagers;

public interface IPdfService
{
    Task<byte[]> GenerateTicketPdfAsync(Guid id);
}
