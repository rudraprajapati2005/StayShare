using System;

namespace StayShare.Models
{
    public class ParentLink
    {
        public int ParentLinkId { get; set; }
        public int ParentId { get; set; }
        public int ChildId { get; set; }
        public DateTime LinkedAt { get; set; }

        // Navigation
        public User Parent { get; set; }
        public User Child { get; set; }
    }

}
