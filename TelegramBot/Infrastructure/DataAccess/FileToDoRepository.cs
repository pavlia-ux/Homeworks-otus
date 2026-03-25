using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {
        private readonly string _directoryName;

        public enum IndexOperation
        {
            Update, UpdateAll
        }

        public FileToDoRepository(string directoryName) 
        {
            _directoryName = directoryName;
            if (!Directory.Exists(directoryName)) 
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return (await GetTasks(userId, ct)).Where(x => x.User.UserId == userId).ToList();
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return (await GetTasks(userId, ct)).Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var items = (await GetTasks(userId, ct)).Where(x => x.User.UserId == userId);
            return items.Where(x => predicate(x)).ToList();
        }
        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            if (!(await DirectoryIndexes.GetTaskIndexes()).ContainsKey(id.ToString()))
                throw new ArgumentException("Такой задачи нет");
            ToDoItem item;
            using (FileStream stream = File.OpenRead(Path.Combine(_directoryName, (await DirectoryIndexes.GetTaskIndexes())[id.ToString()], $"{id.ToString()}.json")))
            {
                item = await JsonSerializer.DeserializeAsync<ToDoItem>(stream, cancellationToken: ct);
            }
            return item;
        }
        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            string pathToUser = Path.Combine(_directoryName, $"{item.User.UserId}.json");
            string pathToTask = Path.Combine(_directoryName, $"{item.User.UserId}", $"{item.Id}.json");

            using (FileStream stream = File.Create(pathToTask))
            {
                await JsonSerializer.SerializeAsync(stream, item, cancellationToken: ct);
            }

            await DirectoryIndexes.AddTaskIndex(item.Id.ToString(), item.User.UserId.ToString());
        }
        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            if (!(await DirectoryIndexes.GetTaskIndexes()).ContainsKey(item.Id.ToString()))
                throw new ArgumentException("Такой задачи нет");
            string pathToTask = Path.Combine(_directoryName, item.User.UserId.ToString(), $"{item.Id.ToString()}.json");
            ToDoItem itemChanging = null;
            using (FileStream stream = new FileStream(pathToTask, FileMode.Open, FileAccess.Read))
            {
                itemChanging = await JsonSerializer.DeserializeAsync<ToDoItem>(stream, cancellationToken: ct);
                itemChanging.State = ToDoItemState.Completed;
            }
            using (FileStream stream = new FileStream(pathToTask, FileMode.Create, FileAccess.Write))
            {
                await JsonSerializer.SerializeAsync(stream, itemChanging, cancellationToken: ct);
            }
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            if (!(await DirectoryIndexes.GetTaskIndexes()).ContainsKey(id.ToString()))
                throw new ArgumentException("Такой задачи нет");
            string path = Path.Combine(_directoryName, (await DirectoryIndexes.GetTaskIndexes())[id.ToString()], $"{id.ToString()}.json");
            File.Delete(path);
            await DirectoryIndexes.RemoveTaskIndex(id.ToString());
        }
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            return (await GetTasks(userId, ct)).Any(x => x.User.UserId == userId && x.Name == name);
        }
        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            return (await GetTasks(userId, ct)).Count(x => x.State == ToDoItemState.Active);
        }

        private async Task<List<ToDoItem>> GetTasks(Guid userId, CancellationToken ct)
        {
            List<ToDoItem> result = new List<ToDoItem>();
            string[] files = Directory.GetFiles(Path.Combine(_directoryName, userId.ToString()));
            for (int i = 0; i < files.Length; i++)
            {
                using (FileStream stream = File.OpenRead(files[i]))
                {
                    result.Add(await JsonSerializer.DeserializeAsync<ToDoItem>(stream, cancellationToken: ct));
                }
            }
            return result;
        }
    }
}
