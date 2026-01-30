using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;

using Otus.ToDoList.ConsoleBot.Types;

using static Homeworks_otus.Core.Entities.ToDoItem;

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
        
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId, x => x.Name.StartsWith(namePrefix));
        }
        public ToDoItem Add(ToDoUser user, string name)
        {
            ToDoItem toDoItem = new ToDoItem(user, name);
            
            if (_toDoRepository.CountActive(user.UserId) >= MaxQuantity)
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

            _toDoRepository.Add(toDoItem);
            return toDoItem;
        }
        public void MarkAsCompleted(Guid id)
        {
            var task = _toDoRepository.Get(id);

            if (task != null)
                _toDoRepository.Update(task);
        }
        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public static bool ValidateString(string? str)
        {
            bool strIsNullOrWhiteSpace = string.IsNullOrWhiteSpace(str);
            return strIsNullOrWhiteSpace;
        }

        private bool IsDuplicate(ToDoUser user, ToDoItem toDoItem)
        {
            foreach (ToDoItem item in _toDoRepository.GetAllByUserId(user.UserId))
            {
                if (item.Name == toDoItem.Name) 
                {
                    return true;
                }
            }
            return false;
        }
    }
}
