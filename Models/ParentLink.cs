using System;
using System.ComponentModel.DataAnnotations;

namespace StayShare.Models
{
    public enum ParentLinkStatus
    {
        Pending,
        Accepted,
        Declined
    }

    public enum RelationshipType
    {
        Mother,
        Father,
        Sister,
        Brother,
        Guardian,
        Other
    }

    public class ParentLink
    {
        public int ParentLinkId { get; set; }
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        
        [Required]
        public ParentLinkStatus Status { get; set; } = ParentLinkStatus.Pending;
        
        [Required]
        public RelationshipType RelationshipType { get; set; }
        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
        public DateTime? LinkedAt { get; set; }
        
        [StringLength(500)]
        public string Message { get; set; }
        
        [StringLength(500)]
        public string ResponseMessage { get; set; }

        // Navigation
        public User Parent { get; set; }
        public User Child { get; set; }
    }
}
