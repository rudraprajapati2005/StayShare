using System;
using System.ComponentModel.DataAnnotations;

namespace StayShare.Models
{
    public enum BookingStatus
    {
        Pending = 0,
        Accepted = 1,
        Declined = 2,
        Expired = 3
    }

    public class BookingRequest
    {
        public int BookingRequestId { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room Room { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime MoveInDate { get; set; }

        [Range(1, 36)]
        public int Months { get; set; }

        public BookingStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        [StringLength(500)]
        public string Note { get; set; }
    }
}



