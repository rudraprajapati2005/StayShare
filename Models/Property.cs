using System.Collections.Generic;

namespace StayShare.Models
{
    public class Property
    {
        public int PropertyId { get; set; }
        public string Name { get; set; } // e.g., "Sunrise PG"
        public string Type { get; set; } // "PG", "Hostel", "House"
        public string Category { get; set; } // "Male", "Female", "Unisex"
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string OwnerContact { get; set; }
        public string Amenities { get; set; } // Optional JSON or comma-separated
        public bool IsVerified { get; set; }

        // Navigation
        public ICollection<Room> Rooms { get; set; }
    }

}
