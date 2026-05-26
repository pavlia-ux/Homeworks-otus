using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct);
        Task<IReadOnlyList<ToDoUser>> GetAllUsers(CancellationToken ct);
        Task AddAsync(ToDoUser user, CancellationToken ct);
    }
}
