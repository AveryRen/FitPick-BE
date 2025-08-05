using FitPick.Repository.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitPick.Repository.Basic
{
    public class GenericRepository<T> where T : class
    {
        protected FitPickContext _context;

        public GenericRepository()
        {
            _context ??= new FitPickContext();
        }

        public GenericRepository(FitPickContext context)
        {
            _context = context;
        }
        public async Task<TKey> CreateReturnKeyAsync<TKey>(T entity)
        {
            try
            {
                // Kiểm tra entity null
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                // Thêm entity vào DbSet
                await _context.Set<T>().AddAsync(entity);

                // Lưu thay đổi vào database
                await _context.SaveChangesAsync();

                // Lấy metadata của khóa chính
                var keyProperty = _context.Entry(entity).Metadata.FindPrimaryKey()?.Properties[0];
                if (keyProperty == null)
                    throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a primary key defined.");

                // Lấy giá trị khóa chính
                var keyValue = _context.Entry(entity).Property(keyProperty.Name).CurrentValue;

                // Ép kiểu về TKey
                return (TKey)Convert.ChangeType(keyValue, typeof(TKey));
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating {typeof(T).Name}: {ex.Message}", ex);
            }
        }
        public List<T> GetAll()
        {
            return _context.Set<T>().ToList();
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }
        public void Create(T entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }


        public async Task<int> CreateAsync(T entity)
        {
            _context.Add(entity);
            return await _context.SaveChangesAsync();
        }


        public async Task<int> CreateAsyncWithCheckExist(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Get the primary key property
            var keyProperty = _context.Entry(entity).Metadata.FindPrimaryKey()?.Properties[0];
            if (keyProperty == null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} does not have a primary key defined.");

            // Get the key value
            var keyValue = _context.Entry(entity).Property(keyProperty.Name).CurrentValue;

            // Check if entity already exists
            var existingEntity = await _context.Set<T>().FindAsync(keyValue);
            if (existingEntity != null)
                throw new InvalidOperationException($"Entity {typeof(T).Name} with key {keyValue} already exists.");

            await _context.Set<T>().AddAsync(entity);
            return await _context.SaveChangesAsync();
        }


        public void Update(T entity)
        {
            //// Turning off Tracking for UpdateAsync in Entity Framework
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            _context.SaveChanges();
        }

        public async Task<int> UpdateAsync(T entity)
        {
            //// Turning off Tracking for UpdateAsync in Entity Framework
            _context.ChangeTracker.Clear();
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
            return await _context.SaveChangesAsync();
        }

        public bool Remove(T entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public T GetById(string code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(string code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        /*
        https://guidgenerator.com/
        daacb4fb-ff73-46ef-98f1-4af9aab2a30a
         */
        public T GetById(Guid code)
        {
            return _context.Set<T>().Find(code);
        }

        public async Task<T> GetByIdAsync(Guid code)
        {
            return await _context.Set<T>().FindAsync(code);
        }

        #region Separating asigned entity and save operators        

        public void PrepareCreate(T entity)
        {
            _context.Add(entity);
        }

        public void PrepareUpdate(T entity)
        {
            var tracker = _context.Attach(entity);
            tracker.State = EntityState.Modified;
        }

        public void PrepareRemove(T entity)
        {
            _context.Remove(entity);
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion Separating asign entity and save operators
    }
}
