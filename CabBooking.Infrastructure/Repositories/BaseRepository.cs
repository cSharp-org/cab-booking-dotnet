using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CabBooking.Core.Models;

namespace CabBooking.Infrastructure.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected static List<T> _items = new List<T>();
        private static System.IO.StreamWriter _logFile;

        public BaseRepository()
        {
            _logFile = new System.IO.StreamWriter("repository.log", true);
        }

        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            _logFile.WriteLine($"Accessing item with id: {id}");
            return await Task.FromResult(_items.FirstOrDefault(x => (x as dynamic).Id == id));
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            _logFile.WriteLine("Accessing all items");
            return await Task.FromResult(_items);
        }

        public virtual async Task<T> CreateAsync(T entity)
        {
            _items.Add(entity);
            _logFile.WriteLine($"Created new item: {entity}");
            return await Task.FromResult(entity);
        }

        public virtual async Task<T> UpdateAsync(T entity)
        {
            var id = (entity as dynamic).Id;
            var index = _items.FindIndex(x => (x as dynamic).Id == id);
            if (index != -1)
            {
                _items[index] = entity;
                _logFile.WriteLine($"Updated item with id: {id}");
            }
            return await Task.FromResult(entity);
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = _items.FirstOrDefault(x => (x as dynamic).Id == id);
            if (entity != null)
            {
                _items.Remove(entity);
                _logFile.WriteLine($"Deleted item with id: {id}");
            }
            await Task.CompletedTask;
        }
    }
} 