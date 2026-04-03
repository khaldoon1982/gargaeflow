using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IInspectionService
{
    Task<IEnumerable<InspectionDto>> GetAllAsync();
    Task<InspectionDto?> GetByIdAsync(int id);
    Task<IEnumerable<InspectionDto>> GetByVehicleIdAsync(int vehicleId);
    Task<IEnumerable<InspectionDto>> GetExpiredAsync();
    Task<IEnumerable<InspectionDto>> GetDueSoonAsync(int days = 30);
    Task<InspectionDto> CreateAsync(CreateInspectionDto dto);
    Task<InspectionDto> UpdateAsync(UpdateInspectionDto dto);
    Task DeleteAsync(int id);
}
