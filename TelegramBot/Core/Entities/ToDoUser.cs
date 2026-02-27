using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.Core.Entities
{
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public long TelegramUserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; }
        public ToDoUser() { }
        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            TelegramUserId = telegramUserId;
            TelegramUserName = telegramUserName;
            UserId = Guid.NewGuid();
            RegisteredAt = DateTime.UtcNow;
        }
    }
}
