using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Domain.Models
{
    public class Match
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        [Required]
        public string VideoUrl { get; set; } = string.Empty;
        [Required]
        public string MatchStatus { get; set; } = string.Empty;    // Live , Replay, Cancelled, Upcoming
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        [Required]
        public string CreatedById { get; set; } = string.Empty; // FK to ApplicationUser
        [ForeignKey("CreatedById")]
        public ApplicationUser? User { get; set; } // Navigation property
    }
}
