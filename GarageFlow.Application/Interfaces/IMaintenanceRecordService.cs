using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IMaintenanceRecordService
{
    Task<IEnumerable<MaintenanceRecordDto>> GetAllAsync();
    Task<MaintenanceRecordDto?> GetByIdAsync(int id);
    Task<IEnumerable<MaintenanceRecordDto>> GetByVehicleIdAsync(int vehicleId);
    Task<MaintenanceRecordDto> CreateAsync(CreateMaintenanceRecordDto dto);
    Task<MaintenanceRecordDto> UpdateAsync(UpdateMaintenanceRecordDto dto);
    Task DeleteAsync(int id);
}
