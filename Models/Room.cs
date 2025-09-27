using System.Collections.Generic;

namespace StayShare.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } // e.g., "101", "B2"
        public string RoomType { get; set; } // "Single", "Double", "Triple", etc.
        public decimal RentPerMonth { get; set; }
        public int Capacity { get; set; }
        public string Amenities { get; set; } // Optional amenities for the room
        public bool IsAvailable { get; set; }

        public int PropertyId { get; set; }
        public Property Property { get; set; }

        // Navigation
        // USER... 
        public ICollection<RoomOccupancy> Occupants { get; set; }
        public ICollection<RoomOccupancy> CurrentlyStaying { get; set; }
    }

}
