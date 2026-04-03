using GarageFlow.Application.DTOs;

namespace GarageFlow.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> SearchAsync(string query);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto);
    Task DeleteAsync(int id);
}
