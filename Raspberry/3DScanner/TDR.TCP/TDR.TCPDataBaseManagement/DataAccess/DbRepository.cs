using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TDR.TCPDataBaseManagement.Models;

namespace TDR.TCPDataBaseManagement.DataAccess
{
    public class DbRepository<T> where T : BaseModel
    {
        private readonly DbContextTdr _context;
        private readonly MySqlConnection _connection;

        public DbRepository(DbContextTdr context, MySqlConnection connection)
        {
            _context = context;
            _connection = connection;
        }

        public bool Usable()
        {
            return _connection.State == ConnectionState.Open;
        }

        #region Sync Methods
        public ICollection<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }

        public bool Any(Expression<Func<T, bool>> match)
        {
            return _context.Set<T>().Any(match);
        }

        public T Get(Guid id)
        {
            return _context.Set<T>().Find(id);
        }

        public T Find(Expression<Func<T, bool>> match)
        {
            return _context.Set<T>().SingleOrDefault(match);
        }

        public ICollection<T> FindAll(Expression<Func<T, bool>> match)
        {
            return _context.Set<T>().Where(match).ToList();
        }

        public DbRepository<T> Add(T t)
        {
            //MySqlTransaction transaction = _connection.BeginTransaction();
            //_context.Database.UseTransaction(transaction);

            _context.Set<T>().Add(t);

            _context.SaveChanges();
            //transaction.Commit();

            return this;
        }

        public DbRepository<T> AddRange(IEnumerable<T> t)
        {
            //MySqlTransaction transaction = _connection.BeginTransaction();
            //_context.Database.UseTransaction(transaction);
            foreach (T oneT in t)
            {
                _context.Set<T>().Add(oneT);
            }
            _context.SaveChanges();
            //transaction.Commit();
            return this;
        }

        public T Update(T updated, Guid key)
        {
            if (updated == null)
                return null;

            //MySqlTransaction transaction = _connection.BeginTransaction();
            //_context.Database.UseTransaction(transaction);
            T existing = _context.Set<T>().Find(key);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(updated);
                _context.SaveChanges();
            }
            //transaction.Commit();
            return existing;
        }

        public void Delete(T t)
        {
            //MySqlTransaction transaction = _connection.BeginTransaction();
            //_context.Database.UseTransaction(transaction);
            _context.Set<T>().Remove(t);
            _context.SaveChanges();
            //transaction.Commit();
        }

        public bool Delete(Guid key)
        {
            //MySqlTransaction transaction = _connection.BeginTransaction();
            //_context.Database.UseTransaction(transaction);

            var entity = _context.Set<T>().SingleOrDefault(t => t.Id == key);
            if (entity == null) return false;

            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
            //transaction.Commit();
            return true;
        }

        public int Count()
        {
            return _context.Set<T>().Count();
        }
        #endregion

        #region Async methods

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> match)
        {
            return await _context.Set<T>().AnyAsync(match);
        }
        public async Task<ICollection<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> GetAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<T> AddAsync(T t)
        {
            _context.Set<T>().Add(t);
            await _context.SaveChangesAsync();
            return t;
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> match)
        {
            return await _context.Set<T>().SingleOrDefaultAsync(match);
        }

        public async Task<ICollection<T>> FindAllAsync(Expression<Func<T, bool>> match)
        {
            return await _context.Set<T>().Where(match).ToListAsync();
        }

        public async Task<T> UpdateAsync(T updated)
        {
            if (updated == null)
                return null;

            T existing = await _context.Set<T>().FindAsync(updated.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(updated);
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        public async Task<int> DeleteAsync(T t)
        {
            _context.Set<T>().Remove(t);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task<T> AddOrUpdateAsync(T entity)
        {
            if (entity == null)
                return null;

            T existing = await _context.Set<T>().FindAsync(entity.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return existing;
            }
            var inserted = await AddAsync(entity);
            return inserted;
        }
        #endregion
    }
}
