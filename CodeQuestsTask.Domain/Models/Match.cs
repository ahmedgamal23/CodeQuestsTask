using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Domain.Models
{
    public class Match
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string MatchStatus { get; set; } = string.Empty;    // Live , Replay, Cancelled, Upcoming
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public string CreatedBy { get; set; } = string.Empty; // FK to ApplicationUser
        public ApplicationUser? User { get; set; } // Navigation property
    }
}
