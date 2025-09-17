using System.Collections.Generic;
using System;

namespace StayShare.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Student", "Parent", "Owner", "Admin"
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public UserProfile Profile { get; set; }
        public ICollection<RoomOccupancy> RoomOccupancies { get; set; }
        public ICollection<ParentLink> ParentLinks { get; set; }
    }

}
