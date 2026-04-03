using GarageFlow.Application.Interfaces;
using GarageFlow.Domain.Entities;
using GarageFlow.Domain.Enums;
using GarageFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Persistence.Repositories;

public class SyncQueueRepository : ISyncQueueRepository
{
    private readonly GarageFlowDbContext _context;

    public SyncQueueRepository(GarageFlowDbContext context) { _context = context; }

    public async Task<List<SyncQueueEntry>> GetPendingAsync(int batchSize = 50)
    {
        return await _context.SyncQueue
            .Where(s => s.Status == SyncQueueStatus.Pending)
            .OrderBy(s => s.CreatedAtUtc)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task MarkCompletedAsync(int id)
    {
        var entry = await _context.SyncQueue.FindAsync(id);
        if (entry is null) return;
        entry.Status = SyncQueueStatus.Completed;
        entry.ProcessedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkFailedAsync(int id, string error)
    {
        var entry = await _context.SyncQueue.FindAsync(id);
        if (entry is null) return;
        entry.Status = SyncQueueStatus.Failed;
        entry.ErrorMessage = error;
        entry.RetryCount++;
        entry.ProcessedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task IncrementRetryAsync(int id)
    {
        var entry = await _context.SyncQueue.FindAsync(id);
        if (entry is null) return;
        entry.RetryCount++;
        await _context.SaveChangesAsync();
    }
}
