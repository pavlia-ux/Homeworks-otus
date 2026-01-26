using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;
using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.Core.Services
{
    public class ToDoService : IToDoService
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
        public ToDoItem Add(ToDoUser user, string name)
        {
            ToDoItem toDoItem = new ToDoItem(user, name);
            
            if (_toDoItems.Count >= MaxQuantity)
            {
                throw new TaskCountLimitException(MaxQuantity);
            }

            if (toDoItem.Name.Length > MaxLength)
            {
                throw new TaskLengthLimitException(toDoItem.Name.Length, MaxLength);
            }

            if (IsDuplicate(user, toDoItem))
            {
                throw new DuplicateTaskException(name);
            }

            if (ValidateString(name) == true)
            {
                throw new Exception("Вы ввели пробелы или пустую строку");
            }

            _toDoItems.Add(toDoItem);
            return toDoItem;
        }
        public void MarkAsCompleted(Guid id)
        {
            foreach (ToDoItem toDo in _toDoItems)
            {
                if (toDo.Id == id)
                {
                    toDo.State = ToDoItemState.Completed;
                    toDo.StateChangedAt = DateTime.UtcNow;
                    break;
                }
            }
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

        public static bool ValidateString(string? str)
        {
            bool strIsNullOrWhiteSpace = string.IsNullOrWhiteSpace(str);
            return strIsNullOrWhiteSpace;
        }

        private bool IsDuplicate(ToDoUser user, ToDoItem toDoItem)
        {
            foreach (ToDoItem item in _toDoItems)
            {
                if (item.Name == toDoItem.Name && user == item.User) 
                {
                    return true;
                }
            }
            return false;
        }
    }
}
