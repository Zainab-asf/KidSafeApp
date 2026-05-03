using KidSafeApp.Backend.Data;
using KidSafeApp.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KidSafeApp.Backend.Repositories.Users;

public sealed class UserRepository : IUserRepository
{
    private readonly DataContext _dataContext;

    public UserRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public IQueryable<User> Query() => _dataContext.Users;

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        _dataContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken) =>
        _dataContext.Users.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public Task<bool> UsernameExistsAsync(string username, int? excludingUserId, CancellationToken cancellationToken) =>
        _dataContext.Users.AsNoTracking()
            .AnyAsync(u => u.Username == username && (!excludingUserId.HasValue || u.Id != excludingUserId.Value), cancellationToken);

    public Task AddAsync(User user, CancellationToken cancellationToken) =>
        _dataContext.Users.AddAsync(user, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dataContext.SaveChangesAsync(cancellationToken);
}
