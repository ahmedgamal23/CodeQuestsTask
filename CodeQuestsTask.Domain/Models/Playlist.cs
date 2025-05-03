using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Domain.Models
{
    public class Playlist
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; } = string.Empty; // FK to ApplicationUser
        public ApplicationUser? User { get; set; } // Navigation property
        [Required]
        public int MatchId { get; set; } // FK to Match
        public Match? Match { get; set; } // Navigation property
    }
}
