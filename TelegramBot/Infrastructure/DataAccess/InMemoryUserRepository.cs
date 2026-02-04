using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

using Otus.ToDoList.ConsoleBot.Types;

namespace Homeworks_otus.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();
       
        public ToDoUser? GetUser(Guid userId)
        {
            return _users.FirstOrDefault(user => user.UserId == userId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
        }
        public void Add(ToDoUser user)
        {
            _users.Add(user);
        }
    }
}
