using System;
using System.ComponentModel.DataAnnotations;

namespace StayShare.Models
{
    public class EditProfileModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        // Role and Password are not required for profile editing
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }

        // Profile information
        public UserProfile Profile { get; set; }
    }
}
