using System;
using System.ComponentModel.DataAnnotations;

namespace StayShare.Models
{
    public class UserProfile
    {
        [Key]
        public int UserId { get; set; } // Foreign key to User

        // Identity & Contact
        public string Gender { get; set; } // "Male", "Female", "Other"
        public DateTime DateOfBirth { get; set; }
        public string ContactNumber { get; set; }
        public string ProfileImageUrl { get; set; }

        // Preferences
        public string PreferredGender { get; set; } // For roommate matching
        public int MaxBudget { get; set; }
        public string PreferredLocation { get; set; }

        // Personality & Bio
        public string Interests { get; set; } // Comma-separated or JSON
        public string Bio { get; set; }

        // Verification & Trust
        public bool IsCollegeVerified { get; set; }
        public string CollegeIdImageUrl { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdated { get; set; }

        // Navigation (optional if you want reverse access)
        public User User { get; set; }
    }
}
