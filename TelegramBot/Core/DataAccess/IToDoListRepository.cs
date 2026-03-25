using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.Entities;

namespace Homeworks_otus.TelegramBot.Core.DataAccess
{
    public interface IToDoListRepository
    {
        Task<ToDoList?> Get(Guid id, CancellationToken ct); //Если спика нет, то возвращает null
        Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct);
        Task Add(ToDoList list, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct); //Проверяет, если ли у пользователя список с таким именем
    }
}
