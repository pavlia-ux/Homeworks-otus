using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();
       
        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            return _users.FirstOrDefault(user => user.UserId == userId);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            return _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
        }
        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            _users.Add(user);
        }
    }
}
