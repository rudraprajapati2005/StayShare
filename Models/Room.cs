using System.Collections.Generic;

namespace StayShare.Models
{
    public class Room
    {
        public int RoomId { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Pincode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Capacity { get; set; }
        public List<User> Occupants { get; set; }
    }
}
