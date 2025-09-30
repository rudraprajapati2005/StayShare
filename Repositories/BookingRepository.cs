using Microsoft.EntityFrameworkCore;
using StayShare.Data;
using StayShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BookingRequest> GetByIdAsync(int id)
        {
            return await _context.BookingRequests
                .Include(b => b.Room).ThenInclude(r => r.Property)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BookingRequestId == id);
        }

        public async Task<IEnumerable<BookingRequest>> GetByResidentAsync(int userId)
        {
            return await _context.BookingRequests
                .Include(b => b.Room).ThenInclude(r => r.Property)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<BookingRequest>> GetByHostAsync(string ownerEmail)
        {
            return await _context.BookingRequests
                .Include(b => b.Room).ThenInclude(r => r.Property)
                .Include(b => b.User)
                .Where(b => b.Room.Property.OwnerContact != null && b.Room.Property.OwnerContact.ToLower() == ownerEmail.ToLower())
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(BookingRequest request)
        {
            await _context.BookingRequests.AddAsync(request);
        }

        public async Task UpdateAsync(BookingRequest request)
        {
            _context.BookingRequests.Update(request);
        }
    }
}



