using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

using Otus.ToDoList.ConsoleBot.Types;

namespace Homeworks_otus.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ToDoUser user = new ToDoUser(telegramUserId, telegramUserName);
            _userRepository.Add(user);
            return user;
        }
        public ToDoUser? GetUser(Guid UserId)
        {
            return _userRepository.GetUser(UserId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
        }
    }
}
