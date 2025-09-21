using System.Collections.Generic;
using System.Threading.Tasks;
using StayShare.Models;

namespace StayShare.Repositories
{
    public interface IOccupancyRepository
    {
        Task<RoomOccupancy> GetOccupancyByIdAsync(int id);
        Task<IEnumerable<RoomOccupancy>> GetOccupanciesByUserIdAsync(int userId);
        Task<IEnumerable<RoomOccupancy>> GetOccupanciesByRoomIdAsync(int roomId);
        Task AddOccupancyAsync(RoomOccupancy occupancy);
        Task UpdateOccupancyAsync(RoomOccupancy occupancy);
        Task SaveChangesAsync();
    }
}
