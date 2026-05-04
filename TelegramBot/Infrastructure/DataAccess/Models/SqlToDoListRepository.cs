using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;

using LinqToDB;
using LinqToDB.Async;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess.Models
{
    public class SqlToDoListRepository : IToDoListRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = await dbContext.ToDoLists
                .FirstOrDefaultAsync(l => l.ForeignId == id, ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Сначала находим UserDatabaseId по ForeignId
            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);

            if (userModel == null)
                return Array.Empty<ToDoList>();

            var models = await AsyncExtensions.ToListAsync(dbContext.ToDoLists
                .Where(l => l.UserId == userModel.Id)
                .LoadWith(u => u.User), ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Сначала находим UserDatabaseId по ForeignId
            var userModel = await dbContext.ToDoUsers.FirstOrDefaultAsync(u => u.ForeignId == list.User.UserId, ct);
            if (userModel == null)
                throw new ArgumentException("Пользователь не найден");
            var model = ModelMapper.MapToModel(list);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.ToDoLists
                .FirstOrDefaultAsync(l => l.ForeignId == id, ct);
            if (model == null)
                throw new ArgumentException("Такого листа не существует");
            await dbContext.DeleteAsync(model, token: ct);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);
            if (userModel == null)
                return false;
            return await dbContext.ToDoLists.AnyAsync(l => l.UserId == userModel.Id && l.ListName == name, ct);
        }
    }
}
