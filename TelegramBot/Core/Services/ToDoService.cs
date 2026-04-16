using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;
using Homeworks_otus.TelegramBot.Core.Entities;

namespace Homeworks_otus.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;
        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        int maxLength = 100;
        int maxQuantity = 100;
        public int MaxLength { get => maxLength; set => maxLength = value; }
        public int MaxQuantity { get => maxQuantity; set => maxQuantity = value; }
        
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await _toDoRepository.GetAllByUserIdAsync(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await _toDoRepository.GetActiveByUserIdAsync(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> GetCompletedByUserIdAsync(Guid userId, CancellationToken ct)
        {
            return await _toDoRepository.GetCompletedByUserIdAsync(userId, ct);
        }
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, string namePrefix, CancellationToken ct)
        {
            return await _toDoRepository.FindAsync(userId, item => item.Name.StartsWith(namePrefix), ct);
        }
        public async Task<ToDoItem> AddAsync(ToDoUser user, string name, DateTime deadLine, ToDoList? list, CancellationToken ct)
        {
            ToDoItem toDoItem = new ToDoItem()
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                State = Entities.Enums.ToDoItemState.Active,
                DeadLine = deadLine,
                ToDoList = toDoList,
                ToDoListDatabaseId = toDoList.DatabaseId,
                UserDatabaseId = user.DatabaseId,
                UserId = user.UserId
            };
            
            if (_toDoRepository.CountActiveAsync(user.UserId, ct).Result >= MaxQuantity)
            {
                throw new TaskCountLimitException(MaxQuantity);
            }

            if (toDoItem.Name.Length > MaxLength)
            {
                throw new TaskLengthLimitException(toDoItem.Name.Length, MaxLength);
            }

            if (IsDuplicate(user, toDoItem, ct))
            {
                throw new DuplicateTaskException(name);
            }

            if (ValidateString(name) == true)
            {
                throw new Exception("Вы ввели пробелы или пустую строку");
            }

            await _toDoRepository.AddAsync(toDoItem, ct);
            return toDoItem;
        }
        public async Task MarkAsCompletedAsync(Guid id, CancellationToken ct)
        {
            var task = await _toDoRepository.GetAsync(id, ct);

            if (task != null)
                await _toDoRepository.UpdateAsync(task, ct);
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            await _toDoRepository.DeleteAsync(id, ct);
        }

        public static bool ValidateString(string? str)
        {
            bool strIsNullOrWhiteSpace = string.IsNullOrWhiteSpace(str);
            return strIsNullOrWhiteSpace;
        }

        private bool IsDuplicate(ToDoUser user, ToDoItem toDoItem, CancellationToken ct)
        {
            foreach (ToDoItem item in _toDoRepository.GetAllByUserIdAsync(user.UserId, ct).Result)
            {
                if (item.Name == toDoItem.Name) 
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            List<ToDoItem> toDoList = new List<ToDoItem>();
            foreach (ToDoItem task in await _toDoRepository.GetAllByUserIdAsync(userId, ct))
            {
                if (task.List != null && task.List.Id == listId)
                    toDoList.Add(task);
            }
            return toDoList;
        }
        public async Task<ToDoItem?> Get(Guid toDoItemId, CancellationToken ct)
        {
            return await _toDoRepository.GetAsync(toDoItemId, ct);
        }
    }
}
