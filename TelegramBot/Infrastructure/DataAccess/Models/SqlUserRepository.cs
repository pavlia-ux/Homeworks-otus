using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess;

using LinqToDB;
using LinqToDB.Async;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess.Models
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(user);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = await dbContext.ToDoUsers
                .FirstOrDefaultAsync(u => u.ForeignId == userId, ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = await dbContext.ToDoUsers
                .FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId, ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }
    }
}
