using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StayShare.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } // e.g., "101", "B2"
        public string RoomType { get; set; } // "Single", "Double", "Triple", etc.
       
        [Column(TypeName = "decimal(10,2)")]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Rent must be greater than 0.")]
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
