namespace GarageFlow.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task<bool> SendReminderEmailAsync(int reminderId, CancellationToken ct = default);
    Task<bool> SendBulkAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken ct = default);
    bool IsConfigured { get; }
}
