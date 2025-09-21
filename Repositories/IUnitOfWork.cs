using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public interface IUnitOfWork
    {
        IRoomRepository Rooms { get; }
        IPropertyRepository Properties { get; }
        IUserRepository Users { get; }
        IOccupancyRepository Occupancies { get; }

        Task<int> CommitAsync();
        void Dispose();
    }
}
