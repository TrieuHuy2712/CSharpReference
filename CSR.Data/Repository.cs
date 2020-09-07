using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSR.Data
{
    public class Repository<T, K> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context = null;
        private DbSet<T> table = null;

        public Repository()
        {
            this._context = new AppDbContext();
            table = _context.Set<T>();
        }

        public async Task<List<T>> GetAll()
        {
            var abc = await table.ToListAsync();
            return abc
        }

        public T GetById(object id)
        {
            throw new NotImplementedException();
        }

        public void Insert(T obj)
        {
            throw new NotImplementedException();
        }

        public void Update(T obj)
        {
            throw new NotImplementedException();
        }

        public void Delete(object id)
        {
            throw new NotImplementedException();
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

#endregion
    }
}