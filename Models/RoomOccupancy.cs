using System;

namespace StayShare.Models
{
    public class RoomOccupancy
    {
        public int RoomOccupancyId { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation
        public Room Room { get; set; }
        public User User { get; set; }
    }
}
