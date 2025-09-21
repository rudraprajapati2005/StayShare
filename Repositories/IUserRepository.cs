using System.Collections.Generic;
using System.Threading.Tasks;
using StayShare.Models;

namespace StayShare.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(int userId);
        Task<User> GetUserByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
    }
}
