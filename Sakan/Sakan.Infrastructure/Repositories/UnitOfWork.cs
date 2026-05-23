using Sakan.Domain.Contracts;
using Sakan.Domain.Entities;
using Sakan.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Infrastructure.Repositories
{
    public class UnitOfWork(ApplicationDbContext _dbContext) : IUnitOfWork
    {
        private readonly Dictionary<string, object> _repositories = [];
        public IRepository<TEntity, Key> GetRepository<TEntity, Key>() where TEntity : BaseEntity<Key>
        {

            var type = typeof(TEntity).Name;
            if (_repositories.TryGetValue(type, out object? value))
            {
                return (IRepository<TEntity, Key>)value;
            }
            else
            {
                var repository = new Repository<TEntity, Key>(_dbContext);
                _repositories.Add(type, repository);
                return repository;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
    }
}

