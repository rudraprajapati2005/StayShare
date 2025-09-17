using System;

namespace StayShare.Models
{
    public enum OccupancyStatus
    {
        Requested,
        Accepted,
        Rejected,
        Left
    }
    public class RoomOccupancy
    {
        public int RoomOccupancyId { get; set; }
        public int RoomId { get; set; }
        public int UserId { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? ExitDate { get; set; }

        public bool IsActive { get; set; } // True only if currently staying
        public OccupancyStatus Status { get; set; }

        public Room Room { get; set; }
        public User User { get; set; }
    }
}
