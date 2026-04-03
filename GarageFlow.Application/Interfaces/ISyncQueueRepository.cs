using GarageFlow.Domain.Entities;

namespace GarageFlow.Application.Interfaces;

public interface ISyncQueueRepository
{
    Task<List<SyncQueueEntry>> GetPendingAsync(int batchSize = 50);
    Task MarkCompletedAsync(int id);
    Task MarkFailedAsync(int id, string error);
    Task IncrementRetryAsync(int id);
}
