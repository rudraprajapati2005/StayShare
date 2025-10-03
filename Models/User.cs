using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace StayShare.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Host|Resident|Guardian|Parent)$", ErrorMessage = "Role must be Host, Resident, Guardian, or Parent")]
        public string Role { get; set; } // "Host", "Resident", "Guardian", "Parent"
        
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public UserProfile Profile { get; set; }
        public ICollection<RoomOccupancy> RoomOccupancies { get; set; }
        public ICollection<ParentLink> ParentLinks { get; set; }
    }

}
