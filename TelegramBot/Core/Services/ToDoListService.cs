using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;
using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;

namespace Homeworks_otus.TelegramBot.Core.Services
{
    public class ToDoListService : IToDoListService
    {
        private readonly IToDoListRepository _toDoListRepository;
        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }
        
        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct) //Размер имени списка не может быть больше 10 символом
                                                                                          //Название списка должно быть уникально в рамках одного ToDoUser
        {
            if (name.Length > 10)
                throw new ArgumentException("Размер имени списка не может быть больше 10 символов!");
            if (await _toDoListRepository.ExistsByName(user.UserId, name, ct))
                throw new DuplicateTaskException(name);
            ToDoList toDoList = new ToDoList()
            {
                Id = Guid.NewGuid(),
                Name = name,
                User = user,
                CreatedAt = DateTime.UtcNow,
                UserDatabaseId = user.DatabaseId
            };
            await _toDoListRepository.Add(toDoList, ct);
            return toDoList;
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            await _toDoListRepository.Delete(id, ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return await _toDoListRepository.Get(id, ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return await _toDoListRepository.GetByUserId(userId, ct);
        }
    }
}
