using CodeQuestsTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Domain.ViewModels
{
    public class MatchPublisherDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public string MatchStatus { get; set; } = string.Empty;    // Live , Replay, Cancelled, Upcoming
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedById { get; set; } = string.Empty;
        public string PublisherName { get; set; } = string.Empty;
    }
}
