using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.Entities;

namespace Homeworks_otus.TelegramBot.Core.DataAccess
{
    public interface IToDoListService
    {
        Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct);
        Task<ToDoList?> Get(Guid id, CancellationToken ct);
        Task Delete(Guid id, CancellationToken ct);
        Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct);
    }
}
