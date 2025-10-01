using StayShare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public interface IBookingRepository
    {
        Task<BookingRequest> GetByIdAsync(int id);
        Task<IEnumerable<BookingRequest>> GetByResidentAsync(int userId);
        Task<IEnumerable<BookingRequest>> GetByHostAsync(string ownerEmail);
        Task AddAsync(BookingRequest request);
        Task UpdateAsync(BookingRequest request);
    }
}



