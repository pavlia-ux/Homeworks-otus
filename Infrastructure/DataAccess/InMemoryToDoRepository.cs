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

using Otus.ToDoList.ConsoleBot.Types;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        private readonly List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            IReadOnlyList<ToDoItem> allToDoItems = _toDoItems.FindAll(x => x.User.UserId == userId).ToList();
            return allToDoItems;
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            IReadOnlyList<ToDoItem> activeToDoItems = _toDoItems.FindAll(x => x.State == ToDoItemState.Active && x.User.UserId == userId);
            return activeToDoItems;
        }      
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            var items = _toDoItems.Where(x => x.User.UserId == userId).ToList();
            return items.Where(predicate).ToList();
        }
        public ToDoItem? Get(Guid id)
        {
            ToDoItem toDoItem = _toDoItems.Find(x => x.Id == id);
            return toDoItem;
        }
        public void Add(ToDoItem item)
        {
            _toDoItems.Add(item);
        }
        public void Update(ToDoItem item)
        {
            var task = _toDoItems.Find(x => x.Id == item.Id);
            if (task is null) 
                return;
            else
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.UtcNow;
        }
        public void Delete(Guid id)
        {
            var task = _toDoItems.Find(x => x.Id == id);
            if (task != null)
                _toDoItems.Remove(task);
        }
        public bool ExistsByName(Guid userId, string name)
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
        public int CountActive(Guid userId)
        {
            IReadOnlyList<ToDoItem> allActiveItems = _toDoItems.FindAll(x => x.User.UserId == userId && x.State == ToDoItemState.Active);
            return allActiveItems.Count;
        }
    }
}
