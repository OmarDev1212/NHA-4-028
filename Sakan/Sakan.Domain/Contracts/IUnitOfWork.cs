using Sakan.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sakan.Domain.Contracts
{
    public interface IUnitOfWork
    {
        IRepository<TEntity, Key> GetRepository<TEntity, Key>() where TEntity : BaseEntity<Key>;
        Task<int> SaveChangesAsync();
    }
}
