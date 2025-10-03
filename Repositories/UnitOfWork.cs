using System;
using System.Threading.Tasks;
using StayShare.Data;

namespace StayShare.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;

        public IRoomRepository Rooms { get; private set; }
        public IPropertyRepository Properties { get; private set; }
        public IUserRepository Users { get; private set; }
        public IOccupancyRepository Occupancies { get; private set; }
        public IBookingRepository Bookings { get; private set; }
        public IParentLinkRepository ParentLinks { get; private set; }

        public UnitOfWork(
            AppDbContext context,
            IRoomRepository roomRepository,
            IPropertyRepository propertyRepository,
            IUserRepository userRepository,
            IOccupancyRepository occupancyRepository,
            IBookingRepository bookingRepository,
            IParentLinkRepository parentLinkRepository)
        {
            _context = context;
            Rooms = roomRepository;
            Properties = propertyRepository;
            Users = userRepository;
            Occupancies = occupancyRepository;
            Bookings = bookingRepository;
            ParentLinks = parentLinkRepository;
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
