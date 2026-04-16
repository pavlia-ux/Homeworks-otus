using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess;

using LinqToDB;
using LinqToDB.Async;
using LinqToDB.Remote;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess.Models
{
    public class SqlToDoRepository : IToDoRepository
    {
        private readonly DataContextFactory _factory;

        public SqlToDoRepository(DataContextFactory factory)
        {
            _factory = factory;
        }

        private async Task<List<ToDoItem>> GetTasks(Guid userId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            var tasks = await dbContext.ToDoItems
                .Where(x => x.User.UserId == userId)
                .LoadWith(x => x.User)
                .LoadWith(x => x.List)
                .LoadWith(x => x.List!.User)
                .ToListAsync(ct);

            return ModelMapper.MapToModel(tasks);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await GetTasks(userId, ct);
            return tasks.Where(x => x.User.UserId == userId).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await GetTasks(userId, ct);
            return tasks.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetCompletedByUserIdAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await GetTasks(userId, ct);
            return tasks.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Completed).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var tasks = await GetTasks(userId, ct);
            return tasks.Where(x => x.User.UserId == userId && predicate(x)).ToList();
        }

        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Поиск задачи по ID
            var tasks = await dbContext.ToDoItems
                .Where(x => x.Id == id)
                .LoadWith(x => x.User)
                .LoadWith(x => x.List)
                .LoadWith(x => x.List!.User)
                .FirstOrDefaultAsync(ct);

            if (tasks == null)
                throw new ArgumentException("Такой задачи нет");

            return ModelMapper.MapToModel(tasks);
        }

        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Добавление новой задачи
            dbContext.ToDoItems.Add(item);
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Проверяем, существует ли задача с таким ID
            var existingItem = await dbContext.ToDoItems
                .Where(x => x.Id == item.Id)
                .FirstOrDefaultAsync(ct);

            if (existingItem == null)
                throw new ArgumentException("Такой задачи нет");

            // Обновление данных задачи
            existingItem.State = ToDoItemState.Completed;

            dbContext.ToDoItems.Update(existingItem);
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();

            // Проверяем, существует ли задача с таким ID
            var tasks = await dbContext.ToDoItems
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync(ct);

            if (tasks == null)
                throw new ArgumentException("Такой задачи нет");

            // Удаляем задачу
            dbContext.ToDoItems.Remove(tasks);
            await dbContext.SaveChangesAsync(ct);
        }

        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            var tasks = await GetTasks(userId, ct);
            return tasks.Any(x => x.User.UserId == userId && x.Name == name);
        }

        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            var tasks = await GetTasks(userId, ct);
            return tasks.Count(x => x.State == ToDoItemState.Active);
        }
    }
}
