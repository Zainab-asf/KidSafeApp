using KidSafeApp.Backend.Data.Entities;

namespace KidSafeApp.Backend.Repositories.Users;

public interface IUserRepository
{
    IQueryable<User> Query();
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> UsernameExistsAsync(string username, int? excludingUserId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
