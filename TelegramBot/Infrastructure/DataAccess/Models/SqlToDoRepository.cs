using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess;

using LinqToDB;
using LinqToDB.Async;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess.Models
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);

            if (userModel == null)
                return Array.Empty<ToDoItem>();

            var models = await AsyncExtensions.ToListAsync(dbContext.ToDoItems
                .Where(i => i.UserId == userModel.Id)
                .LoadWith(i => i.User)
                .LoadWith(i => i.ToDoList), ct);
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);

            if (userModel == null)
                return Array.Empty<ToDoItem>();

            var models = await AsyncExtensions.ToListAsync(dbContext.ToDoItems
                .Where(i => i.UserId == userModel.Id && i.ToDoItemState == (int)ToDoItemState.Active)
                .LoadWith(i => i.User)
                .LoadWith(i => i.ToDoList), ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(Guid userId, DateTime from, DateTime to, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var user = await dbContext.ToDoUsers.FirstOrDefaultAsync(u => u.ForeignId == userId);
            var models = await AsyncExtensions.ToListAsync(dbContext.ToDoItems
                .Where(t => t.UserId == user.Id && t.ToDoItemState == (int)ToDoItemState.Active && (t.DeadLine >= from && t.DeadLine < to))
                .LoadWith(t => t.User)
                .LoadWith(t => t.ToDoList));
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetCompletedByUserIdAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);

            if (userModel == null)
                return Array.Empty<ToDoItem>();

            var models = await AsyncExtensions.ToListAsync(dbContext.ToDoItems
                .Where(i => i.UserId == userModel.Id && i.ToDoItemState == (int)ToDoItemState.Completed)
                .LoadWith(i => i.User)
                .LoadWith(i => i.ToDoList), ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var items = await GetAllByUserIdAsync(userId, ct);
            return items.Where(predicate).ToList();
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await AsyncExtensions.FirstOrDefaultAsync(dbContext.ToDoItems
                .LoadWith(i => i.User)
                .LoadWith(i => i.ToDoList),
                i => i.ForeignId == id, ct);

            return model != null ? ModelMapper.MapFromModel(model) : null;
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = ModelMapper.MapToModel(item);
            await dbContext.InsertAsync(model, token: ct);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.ToDoItems
                .FirstOrDefaultAsync(i => i.ForeignId == item.Id, ct);

            if (model == null)
                throw new ArgumentException("Такой задачи нет");

            model.ToDoItemState = (int)ToDoItemState.Completed;
            model.StateChangedAt = DateTime.UtcNow;
            await dbContext.UpdateAsync(model, token: ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var model = await dbContext.ToDoItems
                .FirstOrDefaultAsync(i => i.ForeignId == id, ct);

            if (model == null)
                throw new ArgumentException("Такой задачи нет");

            await dbContext.DeleteAsync(model, token: ct);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);

            if (userModel == null)
                return false;

            return await dbContext.ToDoItems
                .AnyAsync(i => i.UserId == userModel.Id && i.ItemName == name, ct);
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var userModel = await dbContext.ToDoUsers.FirstAsync(u => u.ForeignId == userId, ct);

            if (userModel == null)
                return 0;

            return await dbContext.ToDoItems
                .CountAsync(i => i.UserId == userModel.Id && i.ToDoItemState == (int)ToDoItemState.Active, ct);
        }
    }
}
