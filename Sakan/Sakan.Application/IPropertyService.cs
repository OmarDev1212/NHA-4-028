using Sakan.Application.DTO;

namespace Sakan.Application
{
    public interface IPropertyService
    {
        public Task<IEnumerable<PropertyDto>> GetAllAsync();
        public Task<PropertyDto?> GetByIdAsync(Guid Id);
        public Task<int> CreateAsync(PropertyDto dto, string currentOwnerId);  
        public Task<bool> DeleteAsync(Guid Id);
        public Task<bool> UpdateAsync(PropertyDto dto);
    }
}
