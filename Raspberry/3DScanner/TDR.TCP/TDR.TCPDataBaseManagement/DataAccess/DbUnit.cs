using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using MySql.Data.MySqlClient;
using TDR.TCPDataBaseManagement.Models;

namespace TDR.TCPDataBaseManagement.DataAccess
{
    public class DbUnit: IDisposable
    {
        private readonly DbContextTdr _context;
        private readonly MySqlConnection _connection;
        private bool _disposed;
        private Dictionary<string, object> _repositoryCache;

        public DbUnit()
        {
            _connection = new MySqlConnection(ConfigurationManager.ConnectionStrings["DbContextTdr"].ConnectionString);
            _connection.Open();
            _context = new DbContextTdr(_connection, false);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public DbRepository<T> Repository<T>() where T : BaseModel
        {
            if (_repositoryCache == null)
            {
                _repositoryCache = new Dictionary<string, object>();
            }

            var type = typeof (T).Name;

            if (!_repositoryCache.ContainsKey(type))
            {
                var repositoryType = typeof (DbRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof (T)), _context,
                    _connection);
                _repositoryCache.Add(type, repositoryInstance);
            }
            return (DbRepository<T>) _repositoryCache[type];
        }

        public DbRepository<T> Repo<T>() where T : BaseModel
        {
            return Repository<T>();
        }

        public DbContext Context => _context;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _repositoryCache?.Clear();
                    _context?.Dispose();
                    _connection?.Close();
                    _connection?.Dispose();
                }
            }
            _disposed = true;
        }
    }
}
