using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly DbContext _context;

    public SettingsService(DbContext context) { _context = context; }

    public async Task<string?> GetAsync(string key)
    {
        var setting = await _context.Set<AppSetting>().FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }

    public async Task SetAsync(string key, string value)
    {
        var setting = await _context.Set<AppSetting>().FirstOrDefaultAsync(s => s.Key == key);
        if (setting is null)
        {
            _context.Set<AppSetting>().Add(new AppSetting { Key = key, Value = value });
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllAsync()
    {
        var all = await _context.Set<AppSetting>().ToListAsync();
        return all.ToDictionary(s => s.Key, s => s.Value);
    }
}
