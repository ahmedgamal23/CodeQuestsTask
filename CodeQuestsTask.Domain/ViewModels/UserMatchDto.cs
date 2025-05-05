using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Domain.ViewModels
{
    public class UserMatchDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public int MatchId { get; set; } 
    }
}
