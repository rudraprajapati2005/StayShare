using Microsoft.EntityFrameworkCore;
using StayShare.Data;
using StayShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public class PropertyRepository : IPropertyRepository
    {
        private readonly AppDbContext _context;

        public PropertyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Property> GetPropertyByIdAsync(int propertyId)
        {
            return await _context.Properties
                .Include(p => p.Rooms)
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);
        }

        public async Task<IEnumerable<Property>> GetPropertiesByCityAsync(string city)
        {
            return await _context.Properties
                .Where(p => p.City.ToLower() == city.ToLower())
                .Include(p => p.Rooms)
                .ToListAsync();
        }

        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _context.Properties
                .Include(p => p.Rooms)
                .ToListAsync();
        }

        public async Task AddPropertyAsync(Property property)
        {
            await _context.Properties.AddAsync(property);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
