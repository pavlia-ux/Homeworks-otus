using System;
using System.Collections.Generic;
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
        int maxLength = 100;
        int maxQuantity = 100;
        public int MaxLength { get => maxLength; set => maxLength = value; }
        public int MaxQuantity { get => maxQuantity; set => maxQuantity = value; }

        private readonly List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            IReadOnlyList<ToDoItem> allToDoItems = _toDoItems.FindAll(n => n.User.UserId == userId);
            return allToDoItems;
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            IReadOnlyList<ToDoItem> activeToDoItems = _toDoItems.FindAll(n => n.State == ToDoItemState.Active && n.User.UserId == userId);
            return activeToDoItems;
        }      
        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            
        }
        public ToDoItem? Get(Guid id)
        {
            ToDoItem toDoItem = _toDoItems.Find(n => n.Id == id);
            return toDoItem;
        }
        public void Add(ToDoItem item)
        {
            if (_toDoItems.Count >= MaxQuantity)
            {
                throw new TaskCountLimitException(MaxQuantity);
            }

            if (item.Name.Length > MaxLength)
            {
                throw new TaskLengthLimitException(item.Name.Length, MaxLength);
            }

            if (IsDuplicate(item))
            {
                throw new DuplicateTaskException(item.Name);
            }

            if (ValidateString(item.Name) == true)
            {
                throw new Exception("Вы ввели пробелы или пустую строку");
            }

            _toDoItems.Add(item);
        }
        public void Update(ToDoItem item)
        {
            
        }
        public void Delete(Guid id)
        {
            foreach (ToDoItem toDo in _toDoItems)
            {
                if (toDo.Id == id)
                {
                    _toDoItems.RemoveAll(i => i.Id == id);
                    break;
                }
            }
        }
        public bool ExistsByName(Guid userId, string name)
        {
            ToDoItem existsItem = _toDoItems.Find(n => n.User.UserId == userId && n.Name == name);
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
            IReadOnlyList<ToDoItem> allActiveItems = _toDoItems.FindAll(n => n.User.UserId == userId && n.State == ToDoItemState.Active);
            return allActiveItems.Count;
        }

        public static bool ValidateString(string? str)
        {
            bool strIsNullOrWhiteSpace = string.IsNullOrWhiteSpace(str);
            return strIsNullOrWhiteSpace;
        }

        private bool IsDuplicate(ToDoItem addItem)
        {
            foreach (ToDoItem item in _toDoItems)
            {
                if (item.Name == addItem.Name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
