using Microsoft.EntityFrameworkCore;
using StayShare.Data;
using StayShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public class OccupancyRepository : IOccupancyRepository
    {
        private readonly AppDbContext _context;

        public OccupancyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<RoomOccupancy> GetOccupancyByIdAsync(int id)
        {
            return await _context.RoomOccupancies
                .Include(ro => ro.User)
                .Include(ro => ro.Room)
                .FirstOrDefaultAsync(ro => ro.RoomOccupancyId == id);
        }

        public async Task<IEnumerable<RoomOccupancy>> GetOccupanciesByUserIdAsync(int userId)
        {
            return await _context.RoomOccupancies
                .Include(ro => ro.Room)
                .Where(ro => ro.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomOccupancy>> GetOccupanciesByRoomIdAsync(int roomId)
        {
            return await _context.RoomOccupancies
                .Include(ro => ro.User)
                .Where(ro => ro.RoomId == roomId)
                .ToListAsync();
        }

        public async Task AddOccupancyAsync(RoomOccupancy occupancy)
        {
            await _context.RoomOccupancies.AddAsync(occupancy);
        }

        public async Task UpdateOccupancyAsync(RoomOccupancy occupancy)
        {
            _context.RoomOccupancies.Update(occupancy);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
