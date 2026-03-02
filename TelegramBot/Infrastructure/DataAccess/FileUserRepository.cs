using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        private readonly string _directoryName;

        public FileUserRepository(string directoryName)
        {
            _directoryName = directoryName;
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            if (!File.Exists(Path.Combine(directoryName, "indexes.json")))
            {
                File.Create(Path.Combine(directoryName, "indexes.json")).Close();
            }
        }

        public async Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
        {
            string[] users = Directory.GetFiles(_directoryName).Where(name => name == $"{userId.ToString()}.json").ToArray();
            ToDoUser? user = null;
            for (int i = 0; i < users.Length; i++)
            {
                using (FileStream stream = File.OpenRead(users[i]))
                {
                    user = await JsonSerializer.DeserializeAsync<ToDoUser>(stream, cancellationToken: ct);
                }
            }
            return user;
        }
        public async Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
        {
            string[] users = Directory.GetFiles(_directoryName, "*.json").Where(name => name.LastIndexOf("indexes.json") == -1).ToArray();
            ToDoUser? user = null;
            for (int i = 0; i < users.Length; i++)
            {
                using (FileStream stream = File.OpenRead(users[i]))
                {
                    user = await JsonSerializer.DeserializeAsync<ToDoUser>(stream, cancellationToken: ct);
                    if (user.TelegramUserId == telegramUserId)
                        return user;
                }
            }
            return user;
        }
        public async Task AddAsync(ToDoUser user, CancellationToken ct)
        {
            using (FileStream stream = File.Create(Path.Combine(_directoryName, $"{user.UserId}.json")))
            {
                await JsonSerializer.SerializeAsync(stream, user, cancellationToken: ct);
            }
            if (!Directory.Exists(Path.Combine(_directoryName, user.UserId.ToString())))
            {
                Directory.CreateDirectory(Path.Combine(_directoryName, user.UserId.ToString()));
            }
        }
    }
}
