using ei_back.Domain.Base.Interfaces;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Domain.Base
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected EIContext _context;

        //Pass the dataset dinamically
        private DbSet<T> _dbSet;

        public GenericRepository(EIContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public T Create(T item)
        {
            try
            {
                _context.Add(item);
                return item;
            }
            catch
            {
                throw;
            }
        }

        public virtual async Task<T> CreateAsync(T item, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.AddAsync(item, cancellationToken);
                return item;
            }
            catch
            {
                throw;
            }
        }

        public List<T> FindAll()
        {
            return _dbSet.ToList();
        }

        public virtual async Task<List<T>> FindAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public T FindById(Guid id)
        {
            return _dbSet.SingleOrDefault(g => g.Id.Equals(id));
        }

        public virtual async Task<T> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.SingleOrDefaultAsync(g => g.Id.Equals(id), cancellationToken);
        }

        public List<T> FindWithPagedSearch(
            string sort,
            int size,
            int page,
            int offset,
            string name,
            string column,
            string table)
        {
            string query = $@"SELECT * FROM {table} t WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(name)) query = query + $" AND t.{column} ILIKE '%{name}%' ";
            query += $" ORDER BY t.{column} {sort} LIMIT {size} OFFSET {offset} ";

            return _dbSet.FromSqlRaw<T>(query).ToList();
        }

        public virtual async Task<List<T>> FindWithPagedSearchAsync(
            string sort,
            int size,
            int page,
            int offset,
            string name,
            string column,
            string table,
            CancellationToken cancellationToken = default)
        {
            string query = $@"SELECT * FROM {table} t WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(name)) query = query + $" AND t.{column} ILIKE '%{name}%' ";
            query += $" ORDER BY t.{column} {sort} LIMIT {size} OFFSET {offset} ";

            return await _dbSet.FromSqlRaw<T>(query).ToListAsync();
        }

        public int GetCount(
            string name,
            string column,
            string table)
        {
            string query = $@"SELECT COUNT(*) FROM {table} t WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(name)) query = query + $" AND t.{column} ILIKE '%{name}%' ";

            var result = "";
            using (var connection = _context.Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    result = command.ExecuteScalar().ToString();
                }
            }
            return int.Parse(result);
        }

        public virtual async Task<int> GetCountAsync(
            string name,
            string column,
            string table,
            CancellationToken cancellationToken = default)
        {
            string query = $@"SELECT COUNT(*) FROM {table} t WHERE 1 = 1";
            if (!string.IsNullOrWhiteSpace(name)) query = query + $" AND t.{column} ILIKE '%{name}%' ";

            var result = "";
            using (var connection = _context.Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    var response = await command.ExecuteScalarAsync();
                    result = response.ToString();
                }
            }
            return int.Parse(result);
        }

        public T Update(T item)
        {
            if (!Exists(item.Id)) return null;

            var result = FindById(item.Id);
            if (result != null)
            {
                try
                {
                    _context.Entry(result).CurrentValues.SetValues(item);
                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else return null;
        }

        public void Delete(Guid id)
        {
            var result = FindById(id);
            if (result != null)
            {
                try
                {
                    _dbSet.Remove(result);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public bool Exists(Guid id)
        {
            return _dbSet.Any(g => g.Id.Equals(id));
        }

        public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(g => g.Id.Equals(id), cancellationToken);
        }
    }
}
