using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    public class FileToDoListRepository : IToDoListRepository
    {
        private readonly string _directoryName;

        public FileToDoListRepository(string directoryName)
        {
            _directoryName = directoryName;
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            if (!(await DirectoryIndexes.GetListIndexes()).ContainsKey(id.ToString())) 
            {
                throw new ArgumentException("Такого листа не существует");
            }
            string pathToList = Path.Combine(_directoryName, (await DirectoryIndexes.GetListIndexes())[id.ToString()], "Lists", $"{id.ToString()}.json");
            ToDoList? toDolist;
            using (FileStream stream = File.OpenRead(pathToList))
            {
                toDolist = await JsonSerializer.DeserializeAsync<ToDoList>(stream, cancellationToken: ct);
            }
            return toDolist;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct) 
        {
            List<ToDoList> ToDolists = new List<ToDoList>();
            foreach (KeyValuePair<string, string> keyValue in (await DirectoryIndexes.GetListIndexes()))
            {
                if (Path.GetFileNameWithoutExtension(keyValue.Value) != userId.ToString())
                {
                    continue;
                }
                string pathToList = Path.Combine(_directoryName, (await DirectoryIndexes.GetListIndexes())[keyValue.Key], "Lists", $"{keyValue.Key}.json");
                using (FileStream stream = File.OpenRead(pathToList))
                {
                    ToDoList list = await JsonSerializer.DeserializeAsync<ToDoList>(stream, cancellationToken: ct);
                    ToDolists.Add(list);
                }
            }
            return ToDolists;
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            string pathToLists = Path.Combine(_directoryName, list.User.UserId.ToString(), "Lists");
            if (!Directory.Exists(pathToLists))
            {
                Directory.CreateDirectory(pathToLists);
            }
            string pathToList = Path.Combine(pathToLists, $"{list.Id.ToString()}.json");
            using (FileStream stream = File.Create(pathToList))
            {
                await JsonSerializer.SerializeAsync(stream, list, cancellationToken: ct);
            }
            await DirectoryIndexes.AddTaskListIndex(list.Id.ToString(), list.User.UserId.ToString());


            //using (FileStream stream = File.Create(Path.Combine(_directoryName, $"{user.UserId}.json")))
            //{
            //    await JsonSerializer.SerializeAsync(stream, user, cancellationToken: ct);
            //}
            //if (!Directory.Exists(Path.Combine(_directoryName, user.UserId.ToString())))
            //{
            //    Directory.CreateDirectory(Path.Combine(_directoryName, user.UserId.ToString()));
            //}
        }

        public async Task Delete(Guid id, CancellationToken ct) 
        {
            if (!(await DirectoryIndexes.GetListIndexes()).ContainsKey(id.ToString()))
            {
                throw new ArgumentException("Такого листа не существует");
            }
            string pathToList = Path.Combine(_directoryName, (await DirectoryIndexes.GetListIndexes())[id.ToString()], "Lists", $"{id.ToString()}.json");
            File.Delete(pathToList);
            await DirectoryIndexes.RemoveTaskListIndex(id.ToString());
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct) 
        {
            string pathToLists = Path.Combine(_directoryName, userId.ToString(), "Lists");
            if (!Directory.Exists(pathToLists))
            {
                Directory.CreateDirectory(pathToLists);
                return false;
            }
            string[] paths = Directory.GetFiles(pathToLists);
            foreach (string path in paths)
            {
                using (FileStream stream = File.OpenRead(path))
                {
                    ToDoList list = await JsonSerializer.DeserializeAsync<ToDoList>(stream, cancellationToken: ct);
                    if (list.Name == name)
                        return true;
                }
            }
            return false;
        }
    }
}
