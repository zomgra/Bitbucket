namespace Bitbucket.Models.Interfaces
{
    public interface IRepository<T>
    {
        Task<bool> Contains(string barcode, CancellationToken cancellationToken);
        Task Add(T value, CancellationToken cancellationToken);
        Task<T> Create(CancellationToken cancellationToken = default);
    }
}
