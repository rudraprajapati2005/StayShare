using Microsoft.EntityFrameworkCore;
using StayShare.Data;
using StayShare.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public class ParentLinkRepository : IParentLinkRepository
    {
        private readonly AppDbContext _context;

        public ParentLinkRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ParentLink> GetByIdAsync(int id)
        {
            return await _context.ParentLinks
                .Include(pl => pl.Parent)
                .Include(pl => pl.Child)
                .FirstOrDefaultAsync(pl => pl.ParentLinkId == id);
        }

        public async Task<IEnumerable<ParentLink>> GetPendingRequestsForUserAsync(int userId)
        {
            return await _context.ParentLinks
                .Include(pl => pl.Parent)
                .Include(pl => pl.Child)
                .Where(pl => (pl.ParentId == userId || pl.ChildId == userId) && pl.Status == ParentLinkStatus.Pending)
                .OrderByDescending(pl => pl.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParentLink>> GetSentRequestsByUserAsync(int userId)
        {
            return await _context.ParentLinks
                .Include(pl => pl.Parent)
                .Include(pl => pl.Child)
                .Where(pl => (pl.ParentId == userId || pl.ChildId == userId))
                .OrderByDescending(pl => pl.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParentLink>> GetAcceptedLinksForUserAsync(int userId)
        {
            return await _context.ParentLinks
                .Include(pl => pl.Parent)
                .Include(pl => pl.Child)
                .Where(pl => (pl.ParentId == userId || pl.ChildId == userId) && pl.Status == ParentLinkStatus.Accepted)
                .OrderByDescending(pl => pl.LinkedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ParentLink>> GetAcceptedLinksForParentAsync(int parentId)
        {
            return await _context.ParentLinks
                .Include(pl => pl.Child)
                .ThenInclude(c => c.RoomOccupancies)
                .ThenInclude(ro => ro.Room)
                .ThenInclude(r => r.Property)
                .Where(pl => pl.ParentId == parentId && pl.Status == ParentLinkStatus.Accepted)
                .OrderByDescending(pl => pl.LinkedAt)
                .ToListAsync();
        }

        public async Task<ParentLink> GetExistingLinkAsync(int parentId, int childId)
        {
            return await _context.ParentLinks
                .Include(pl => pl.Parent)
                .Include(pl => pl.Child)
                .FirstOrDefaultAsync(pl => (pl.ParentId == parentId && pl.ChildId == childId) || 
                                         (pl.ParentId == childId && pl.ChildId == parentId));
        }

        public async Task AddAsync(ParentLink parentLink)
        {
            await _context.ParentLinks.AddAsync(parentLink);
        }

        public async Task UpdateAsync(ParentLink parentLink)
        {
            _context.ParentLinks.Update(parentLink);
        }

        public async Task DeleteAsync(int id)
        {
            var parentLink = await _context.ParentLinks.FindAsync(id);
            if (parentLink != null)
            {
                _context.ParentLinks.Remove(parentLink);
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<bool> HasExistingRequestAsync(int parentId, int childId)
        {
            return await _context.ParentLinks
                .AnyAsync(pl => (pl.ParentId == parentId && pl.ChildId == childId) || 
                               (pl.ParentId == childId && pl.ChildId == parentId));
        }
    }
}

