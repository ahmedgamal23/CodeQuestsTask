using CodeQuestsTask.Application.BaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuestsTask.Application.Interface
{
    public interface IBaseRepository<T, TType> where T : class
    {
        ValueTask<BaseModel<T>> GetAllAsync(
            int pagesize = 10,
            int pageNumber = 1,
            Func<IQueryable<T>, IQueryable<T>>? orderBy = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null,
            Expression<Func<T, bool>>? filter = null
        );
        ValueTask<T?> GetByIdAsync(TType id, Expression<Func<T, bool>>? filter = null);
        ValueTask<BaseModel<T>> GetByName(Expression<Func<T, bool>> filter, int pagesize = 10, int pageNumber = 1);
        ValueTask<BaseModel<T>> GetByMatchStatus(Expression<Func<T, bool>> filter, int pagesize = 10, int pageNumber = 1);
        ValueTask<T> AddAsync(T entity);
        ValueTask<bool> UpdateAsync(T entity, params string[] modified);
        ValueTask<bool> SoftDeleteAsync(TType id);
        ValueTask<bool> DeleteAsync(TType id);
    }
}

