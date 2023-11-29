namespace Bitbucket.Models.Interfaces
{
    public interface IRepository<T>
    {
        Task<bool> Contains(string barcode, CancellationToken cancellationToken);
        Task Add(Shipment shipment, CancellationToken cancellationToken = default);
        Task InjectFromDB();
    }
    public interface IBloomFilterRepository<T> : IRepository<T>
    {
    }

    public interface IShipmentRepository<T> : IRepository<T>
    {
    }
}
