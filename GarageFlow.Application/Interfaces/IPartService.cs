using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IPartService
{
    Task<IEnumerable<PartDto>> GetByMaintenanceRecordIdAsync(int maintenanceRecordId);
    Task<PartDto> CreateAsync(CreatePartDto dto);
    Task DeleteAsync(int id);
}
