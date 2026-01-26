using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private readonly List<ToDoUser> _users = new List<ToDoUser>();

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ToDoUser user = new ToDoUser(telegramUserId, telegramUserName);
            _users.Add(user);
            return user;
        }
        public ToDoUser? GetUser(long telegramUserId)
        {
            ToDoUser? User = _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
            if (User != null)
                return User;
            else
                return null;
        }
    }
}
