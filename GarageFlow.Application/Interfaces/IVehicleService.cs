using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<VehicleDto>> GetAllAsync();
    Task<VehicleDto?> GetByIdAsync(int id);
    Task<IEnumerable<VehicleDto>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<VehicleDto>> SearchByPlateAsync(string plateNumber);
    Task<VehicleDto> CreateAsync(CreateVehicleDto dto);
    Task<VehicleDto> UpdateAsync(UpdateVehicleDto dto);
    Task DeleteAsync(int id);
}
