using CodeQuestsTask.Application.BaseModel;
using CodeQuestsTask.Application.Interface;
using CodeQuestsTask.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Infrastructure.Repository
{
    public class BaseRepository<T, TType> : IBaseRepository<T, TType> where T : class
    {
        private readonly ApplicationDbContext _context;
        private DbSet<T> _dbset;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbset = _context.Set<T>();
        }

        public async ValueTask<BaseModel<T>> GetAllAsync(
           int pagesize = 10,
           int pageNumber = 1,
           Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
           Func<IQueryable<T>, IQueryable<T>>? include = null,
           Expression<Func<T, bool>>? filter = null
        )
        {
            IQueryable<T> query = _dbset.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            if (include != null)
                query = include(query);

            if (orderBy != null)
                query = orderBy(query);

            int totalPages = await query.CountAsync();
            var result = await query.Skip((pageNumber - 1) * pagesize).Take(pagesize).ToListAsync();
            return new BaseModel<T>
            {
                DataList = result,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pagesize,
                success = true
            };
        }

        public async ValueTask<T?> GetByIdAsync(TType id, Expression<Func<T, bool>>? filter = null)
        {
            var query = _dbset.AsNoTracking();

            if (filter != null)
                query = query.Where(filter);

            var entity = await query.FirstOrDefaultAsync(e => EF.Property<TType>(e, "Id")!.Equals(id));
            return entity;
        }

        public async ValueTask<BaseModel<T>> GetByName(Expression<Func<T, bool>> filter , int pagesize = 10, int pageNumber = 1)
        {
            var query = _dbset.Where(filter);
            int totalPages = await query.CountAsync();
            query = query.Skip((pageNumber - 1) * pagesize).Take(pagesize);
            return new BaseModel<T>
            {
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pagesize,
                success = true,
                DataList = await query.ToListAsync()
            };
        }

        public async ValueTask<BaseModel<T>> GetByMatchStatus(Expression<Func<T, bool>> filter, int pagesize = 10, int pageNumber = 1)
        {
            var query = _dbset.Where(filter);
            int totalPages = await query.CountAsync();
            query = query.Skip((pageNumber - 1) * pagesize).Take(pagesize);
            return new BaseModel<T>
            {
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pagesize,
                success = true,
                DataList = await query.ToListAsync()
            };
        }

        public async ValueTask<T> AddAsync(T entity)
        {
            var entry = await _dbset.AddAsync(entity);
            return entry.Entity;
        }

        public ValueTask<bool> UpdateAsync(T entity, params string[] modified)
        {
            var result = _dbset.Attach(entity);
            //_context.Entry(entity).State = EntityState.Modified;
            foreach(var prop in modified)
            {
                _context.Entry(entity).Property(prop).IsModified = true;
            }
            return result.State == EntityState.Modified? new ValueTask<bool>(true) : new ValueTask<bool>(false);
        }

        public async ValueTask<bool> SoftDeleteAsync(TType id)
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

        public async ValueTask<bool> DeleteAsync(TType id)
        {
            var entity = await _dbset.FindAsync(id);
            if (entity == null) return false;
            var result = _dbset.Remove(entity);
            if(result.State.Equals(EntityState.Deleted)) return true;
            return false;
        }
    }
}
