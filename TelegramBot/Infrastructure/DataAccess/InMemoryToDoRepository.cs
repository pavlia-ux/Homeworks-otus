using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> allToDoItems = _toDoItems.FindAll(x => x.User.UserId == userId).ToList();
            return allToDoItems;
        }
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> activeToDoItems = _toDoItems.FindAll(x => x.State == ToDoItemState.Active && x.User.UserId == userId);
            return activeToDoItems;
        }      
        public async Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var items = _toDoItems.Where(x => x.User.UserId == userId).ToList();
            return items.Where(predicate).ToList();
        }
        public async Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
        {
            ToDoItem toDoItem = _toDoItems.Find(x => x.Id == id);
            return toDoItem;
        }
        public async Task AddAsync(ToDoItem item, CancellationToken ct)
        {
            _toDoItems.Add(item);
        }
        public async Task UpdateAsync(ToDoItem item, CancellationToken ct)
        {
            var task = _toDoItems.Find(x => x.Id == item.Id);
            if (task is null) 
                return;
            else
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;
        }
        public async Task DeleteAsync(Guid id, CancellationToken ct)
        {
            var task = _toDoItems.Find(x => x.Id == id);
            if (task != null)
                _toDoItems.Remove(task);
        }
        public async Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
        {
            ToDoItem existsItem = _toDoItems.Find(x => x.User.UserId == userId && x.Name == name);
            if (existsItem != null)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        public async Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
        {
            IReadOnlyList<ToDoItem> allActiveItems = _toDoItems.FindAll(x => x.User.UserId == userId && x.State == ToDoItemState.Active);
            return allActiveItems.Count;
        }
    }
}
