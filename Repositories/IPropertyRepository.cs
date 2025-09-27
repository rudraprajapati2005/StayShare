using System.Collections.Generic;
using System.Threading.Tasks;
using StayShare.Models;

namespace StayShare.Repositories
{
    public interface IPropertyRepository
    {
        Task<Property> GetPropertyByIdAsync(int propertyId);
        Task<IEnumerable<Property>> GetPropertiesByCityAsync(string city);
        Task<IEnumerable<Property>> GetAllPropertiesAsync();
        Task AddPropertyAsync(Property property);
        Task SaveChangesAsync();
    }
}
