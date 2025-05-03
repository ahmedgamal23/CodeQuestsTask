using CodeQuestsTask.Application.Interface;
using CodeQuestsTask.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Infrastructure.Repository
{
    public class BaseRepository<T, Type> : IBaseRepository<T, Type> where T : class
    {
        private readonly ApplicationDbContext _context;
        private DbSet<T> _dbset;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbset = _context.Set<T>();
        }

        public async ValueTask<IEnumerable<T>> GetAllAsync(
           int pagesize = 10,
           int pageNumber = 1,
           Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
           Func<IQueryable<T>, IQueryable<T>>? include = null,
           Expression<Func<T, bool>>? filter = null
        )
        {
            IQueryable<T> query = _dbset.AsNoTracking();
            query = query.Where(e => EF.Property<bool>(e, "IsDeleted") == false);

            if (filter != null)
                query = query.Where(filter);

            if (include != null)
                query = include(query);

            if (orderBy != null)
                query = orderBy(query);

            query = query.Skip((pageNumber - 1) * pagesize).Take(pagesize);
            return await query.ToListAsync();
        }

        public async ValueTask<T?> GetByIdAsync(Type id)
        {
            return await _dbset.FindAsync(id);
        }

        public IEnumerable<T> GetByNameAsync(Expression<Func<T, bool>> filter)
        {
            return _dbset.Where(filter);
        }

        public async ValueTask<T> AddAsync(T entity)
        {
            var entry = await _dbset.AddAsync(entity);
            return entry.Entity;
        }

        public ValueTask<bool> UpdateAsync(T entity)
        {
            var result = _dbset.Attach(entity);
            //_context.Entry(entity).State = EntityState.Modified;
            return result.State == EntityState.Modified? new ValueTask<bool>(true) : new ValueTask<bool>(false);
        }

        public async ValueTask<bool> DeleteAsync(Type id)
        {
            var entity = await _dbset.FindAsync(id);
            if (entity == null) return false;

            var prop = typeof(T).GetProperty("IsDeleted");
            if (prop != null)
            {
                prop.SetValue(entity, true);
                _dbset.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
                return true;
            }

            return false;
        }
    }
}
