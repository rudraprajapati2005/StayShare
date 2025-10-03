using StayShare.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StayShare.Repositories
{
    public interface IParentLinkRepository
    {
        Task<ParentLink> GetByIdAsync(int id);
        Task<IEnumerable<ParentLink>> GetPendingRequestsForUserAsync(int userId);
        Task<IEnumerable<ParentLink>> GetSentRequestsByUserAsync(int userId);
        Task<IEnumerable<ParentLink>> GetAcceptedLinksForUserAsync(int userId);
        Task<IEnumerable<ParentLink>> GetAcceptedLinksForParentAsync(int parentId);
        Task<ParentLink> GetExistingLinkAsync(int parentId, int childId);
        Task AddAsync(ParentLink parentLink);
        Task UpdateAsync(ParentLink parentLink);
        Task DeleteAsync(int id);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<bool> HasExistingRequestAsync(int parentId, int childId);
    }
}

