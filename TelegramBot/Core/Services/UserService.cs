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

        public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
        {
            ToDoUser user = new ToDoUser()
            {
                UserId = Guid.NewGuid(),
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.Now,
                TelegramUserId = telegramUserId
            };
            await _userRepository.AddAsync(user, ct);
            return user;
        }
        public async Task<ToDoUser?> GetUserAsync(Guid UserId, CancellationToken ct)
        {
            return await _userRepository.GetUserAsync(UserId, ct);
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            return await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
        }
    }
}
