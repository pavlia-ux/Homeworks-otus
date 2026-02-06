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
        int MaxLength { get; set; }
        int MaxQuantity { get; set; }
        Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
        Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct);
        //Метод должен возвращать все задачи пользователя, которые начинаются на namePrefix. Для этого нужно использовать метод IToDoRepository.Find
        Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct);
        Task MarkAsCompletedAsync(Guid id, CancellationToken ct);
        Task DeleteAsync(Guid id, CancellationToken ct);
    }
}
