using StayShare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public interface IRoomRepository
    {
        Task<Room> GetRoomByIdAsync(int roomId);
        Task<IEnumerable<Room>> GetAvailableRoomsAsync();
        Task<IEnumerable<Room>> GetRoomsByPropertyIdAsync(int propertyId);
        Task<IEnumerable<RoomOccupancy>> GetCurrentOccupantsAsync(int roomId);
        Task AddRoomAsync(Room room);
        Task UpdateRoomAsync(Room room);
        Task DeleteRoomAsync(int roomId);
        Task SaveChangesAsync();
    }
}
