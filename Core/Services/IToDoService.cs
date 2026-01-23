using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Homeworks_otus.Core.Entities;

namespace Homeworks_otus
{
    public interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId);
        //Возвращает ToDoItem для UserId со статусом Active
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        ToDoItem Add(ToDoUser user, string name);
        void MarkAsCompleted(Guid id);
        void Delete(Guid id);
    }
}
