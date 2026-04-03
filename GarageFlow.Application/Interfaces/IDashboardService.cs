using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync();
}
