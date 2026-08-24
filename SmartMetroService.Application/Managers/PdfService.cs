using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartMetroService.Application.Interfaces.IManagers;
using SmartMetroService.Application.Interfaces.IRepositories;
using SmartMetroService.Domain.Entities;

namespace SmartMetroService.Application.Managers;

public class PdfService : IPdfService
{
    private readonly IUnitOfWork _unitOfWork;

    public PdfService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public async Task<byte[]> GenerateTicketPdfAsync(Guid id)
    {
        var ticket = await _unitOfWork.TicketRepository.GetTicketByIdAsync(id);

        if (ticket is null)
        {
            throw new KeyNotFoundException($"Ticket with id '{id}' was not found.");
        }

        var pdfByte = GeneratePdf(ticket);

        return pdfByte;
    }

    private byte[] GeneratePdf(Ticket ticket)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Standard POS ticket size (80mm width x 100mm height)
                page.Size(80, 100, Unit.Millimetre);

                // Minimal margins for maximum QR visibility
                page.Margin(4);

                // Black & white theme
                page.DefaultTextStyle(x =>
                    x.FontSize(7)
                     .FontColor(Colors.Black));

                page.Content()
                    .Column(column =>
                    {
                        column.Spacing(2);

                        // --- HEADER ---
                        column.Item()
                            .AlignCenter()
                            .Text("SMART METRO")
                            .FontSize(9)
                            .Bold();

                        column.Item()
                            .AlignCenter()
                            .Text("E-TICKET")
                            .FontSize(7)
                            .FontColor(Colors.Grey.Darken2);

                        column.Item()
                            .LineHorizontal(0.3f)
                            .LineColor(Colors.Black);

                        // Spacer
                        column.Item()
                            .Height(2);

                        // --- QR CODE (Main focus) ---
                        if (ticket.QRByte is not null)
                        {
                            column.Item()
                                .AlignCenter()
                                .Width(180)
                                .Height(180)
                                .Image(ticket.QRByte);
                        }

                        column.Item()
                            .AlignCenter()
                            .Text("Scan at Entry Gate & Exit Gate")
                            .Bold()
                            .FontSize(5)
                            .FontColor(Colors.Grey.Darken1);

                        column.Item()
                            .Height(2);

                        // --- DIVIDER ---
                        column.Item()
                            .LineHorizontal(0.3f)
                            .LineColor(Colors.Black);

                        // --- JOURNEY INFORMATION ---
                        column.Item()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span(ticket.FromStation?.StationName ?? "-")
                                    .FontSize(7)
                                    .Bold();

                                text.Span("  →  ")
                                    .FontSize(8)
                                    .Bold();

                                text.Span(ticket.ToStation?.StationName ?? "-")
                                    .FontSize(7)
                                    .Bold();
                            });

                        // --- FARE & EXPIRY ---
                        column.Item()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.Span("Fare: ")
                                    .FontSize(6)
                                    .FontColor(Colors.Grey.Darken1);

                                text.Span($"৳ {ticket.Fare}")
                                    .FontSize(7)
                                    .Bold();

                                text.Span("  |  ")
                                    .FontSize(6);

                                text.Span("Exp: ")
                                    .FontSize(6)
                                    .FontColor(Colors.Grey.Darken1);

                                text.Span(ticket.ExpiryTime?
                                    .ToString("dd MMM HH:mm") ?? "-")
                                    .FontSize(6)
                                    .Bold();
                            });

                        // --- INSTRUCTIONS ---

                        column.Item()
                            .AlignCenter()
                            .Text("Thank you for travelling with Smart Metro")
                            .FontSize(4)
                            .FontColor(Colors.Grey.Darken2);
                    });
            });
        }).GeneratePdf();
    }
}
