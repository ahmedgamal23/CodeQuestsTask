using CodeQuestsTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Application.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IBaseRepository<ApplicationUser, string> UserRepository { get; }
        IBaseRepository<Match, int> MatchRepository { get; }
        IBaseRepository<Playlist, int> PlaylistRepository { get; }
        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
