namespace ei_back.Domain.Base.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        T Create(T item);
        Task<T> CreateAsync(T item, CancellationToken cancellationToken = default);
        List<T> FindAll();
        Task<List<T>> FindAllAsync(CancellationToken cancellationToken = default);
        T FindById(Guid id);
        Task<T> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
        List<T> FindWithPagedSearch(
            string sort,
            int size,
            int page,
            int offset,
            string name,
            string column,
            string table);
        Task<List<T>> FindWithPagedSearchAsync(
            string sort,
            int size,
            int page,
            int offset,
            string name,
            string column,
            string table,
            CancellationToken cancellationToken = default);
        int GetCount(
            string name,
            string column,
            string table);
        Task<int> GetCountAsync(
            string name,
            string column,
            string table,
            CancellationToken cancellationToken = default);
        T Update(T item);
        void Delete(Guid id);
        bool Exists(Guid id);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    }
}
