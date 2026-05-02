using Homeworks_otus.Core.Entities;

namespace Homeworks_otus
{
    public interface IUserService
    {
        Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct);
        Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct);
    }
}
