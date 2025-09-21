using Microsoft.EntityFrameworkCore;
using StayShare.Data;
using StayShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public class RoomRepository : IRoomRepository
    {
        private readonly AppDbContext _context;

        public RoomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Room> GetRoomByIdAsync(int roomId)
        {
            return await _context.Rooms
                .Include(r => r.Property)
                .Include(r => r.Occupants).ThenInclude(o => o.User)
                .FirstOrDefaultAsync(r => r.RoomId == roomId);
        }

        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync()
        {
            return await _context.Rooms
                .Include(r => r.Property)
                .Where(r => r.IsAvailable)
                .ToListAsync();
        }

        public async Task<IEnumerable<RoomOccupancy>> GetCurrentOccupantsAsync(int roomId)
        {
            return await _context.RoomOccupancies
                .Include(o => o.User)
                .Where(o => o.RoomId == roomId && o.Status == OccupancyStatus.Accepted)
                .ToListAsync();
        }

        public async Task AddRoomAsync(Room room)
        {
            await _context.Rooms.AddAsync(room);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
