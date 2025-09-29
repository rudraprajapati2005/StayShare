using Microsoft.EntityFrameworkCore;
using StayShare.Data;
using StayShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        public async Task<IEnumerable<Property>> GetPropertiesByOwnerEmailAsync(string ownerEmail)
        {
            return await _context.Properties
                .Where(p => p.OwnerContact != null && ownerEmail != null && p.OwnerContact.ToLower() == ownerEmail.ToLower())
                .Include(p => p.Rooms)
                .ToListAsync();
        }

        // Haversine-based filtering on SQL side for performance
        public async Task<IEnumerable<Property>> GetNearbyPropertiesAsync(double latitude, double longitude, double radiusKm, string type = null, string category = null)
        {
            // Convert km to meters for clarity (not strictly needed)
            var lat = latitude;
            var lng = longitude;
            var radKm = radiusKm <= 0 ? 5.0 : radiusKm;

            // Haversine formula in SQL using radians
            // distance_km = 6371 * acos(cos(rad(lat1)) * cos(rad(lat2)) * cos(rad(lon2 - lon1)) + sin(rad(lat1)) * sin(rad(lat2)))
            var query = _context.Properties.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }

            // Filter by bounding box first to reduce cost
            const double earthRadiusKm = 6371.0;
            var deltaLat = (radKm / earthRadiusKm) * (180.0 / Math.PI);
            var deltaLng = (radKm / earthRadiusKm) * (180.0 / Math.PI) / Math.Cos(lat * Math.PI / 180.0);
            var minLat = lat - deltaLat;
            var maxLat = lat + deltaLat;
            var minLng = lng - deltaLng;
            var maxLng = lng + deltaLng;

            query = query.Where(p => p.Latitude >= minLat && p.Latitude <= maxLat && p.Longitude >= minLng && p.Longitude <= maxLng);

            // Materialize and finalize with precise haversine on app side
            var prelim = await query.ToListAsync();

            IEnumerable<Property> precise = prelim.Where(p =>
            {
                double dLat = (p.Latitude - lat) * Math.PI / 180.0;
                double dLng = (p.Longitude - lng) * Math.PI / 180.0;
                double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                           Math.Cos(lat * Math.PI / 180.0) * Math.Cos(p.Latitude * Math.PI / 180.0) *
                           Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                double distanceKm = earthRadiusKm * c;
                return distanceKm <= radKm + 1e-6; // include margin
            });

            return precise.ToList();
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
