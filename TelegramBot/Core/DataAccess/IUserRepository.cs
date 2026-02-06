using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.Core.DataAccess
{
    public interface IUserRepository
    {
        Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct);
        Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct);
        Task AddAsync(ToDoUser user, CancellationToken ct);
    }
}
