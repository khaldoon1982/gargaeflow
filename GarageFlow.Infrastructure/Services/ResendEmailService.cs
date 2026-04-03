using System.Net.Http.Json;
using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using GarageFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace GarageFlow.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public bool IsConfigured { get; }

    public ResendEmailService(HttpClient httpClient, IServiceProvider serviceProvider, ILogger logger)
    {
        _httpClient = httpClient;
        _serviceProvider = serviceProvider;
        _logger = logger;

        var apiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";
        _fromEmail = Environment.GetEnvironmentVariable("RESEND_FROM_EMAIL") ?? "noreply@garageflow.nl";
        _fromName = Environment.GetEnvironmentVariable("RESEND_FROM_NAME") ?? "GarageFlow";

        IsConfigured = !string.IsNullOrEmpty(apiKey);

        if (IsConfigured)
        {
            _httpClient.BaseAddress = new Uri("https://api.resend.com/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }
    }

    public async Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.Warning("Resend is niet geconfigureerd (RESEND_API_KEY ontbreekt)");
            return false;
        }

        try
        {
            var payload = new
            {
                from = $"{_fromName} <{_fromEmail}>",
                to = new[] { to },
                subject,
                html = htmlBody
            };

            var response = await _httpClient.PostAsJsonAsync("emails", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.Information("E-mail verzonden naar {To}: {Subject}", to, subject);
                return true;
            }

            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.Error("Resend fout ({Status}): {Error}", response.StatusCode, error);
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Fout bij verzenden e-mail naar {To}", to);
            return false;
        }
    }

    public async Task<bool> SendReminderEmailAsync(int reminderId, CancellationToken ct = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();

        var reminder = await context.Reminders
            .Include(r => r.Customer)
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == reminderId, ct);

        if (reminder is null)
        {
            _logger.Warning("Herinnering {Id} niet gevonden", reminderId);
            return false;
        }

        var email = reminder.Customer.Email;
        if (string.IsNullOrEmpty(email))
        {
            _logger.Warning("Klant {Name} heeft geen e-mailadres", reminder.Customer.DisplayName);
            return false;
        }

        var vehicleInfo = reminder.Vehicle is not null
            ? $"{reminder.Vehicle.Brand} {reminder.Vehicle.Model} ({reminder.Vehicle.PlateNumberOriginal})"
            : "";

        var html = $@"
<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
    <div style='background: #1B5E20; color: white; padding: 20px; border-radius: 8px 8px 0 0;'>
        <h2 style='margin: 0;'>GarageFlow - Herinnering</h2>
    </div>
    <div style='padding: 24px; background: #f9f9f9; border: 1px solid #e0e0e0; border-radius: 0 0 8px 8px;'>
        <p>Beste {reminder.Customer.DisplayName},</p>
        <p>{reminder.Message}</p>
        {(string.IsNullOrEmpty(vehicleInfo) ? "" : $"<p><strong>Voertuig:</strong> {vehicleInfo}</p>")}
        <p><strong>Datum:</strong> {reminder.ReminderDate:dd-MM-yyyy}</p>
        <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 20px 0;'>
        <p style='color: #757575; font-size: 12px;'>Dit is een automatisch bericht van GarageFlow.</p>
    </div>
</div>";

        return await SendAsync(email, $"Herinnering: {reminder.Message}", html, ct);
    }

    public async Task<bool> SendBulkAsync(IEnumerable<string> recipients, string subject, string htmlBody, CancellationToken ct = default)
    {
        var success = true;
        foreach (var to in recipients)
        {
            var result = await SendAsync(to, subject, htmlBody, ct);
            if (!result) success = false;
        }
        return success;
    }
}
