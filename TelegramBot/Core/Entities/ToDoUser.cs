using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.Core.Entities
{
    public class ToDoUser
    {
        private Guid _userId;
        public long telegramUserId;
        public string telegramUserName;
        private DateTime _registeredAt;
        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            this.telegramUserId = telegramUserId;
            this.telegramUserName = telegramUserName;
            _userId = Guid.NewGuid();
            _registeredAt = DateTime.UtcNow;
        }
        public Guid UserId
        {
            get { return _userId; }
        }
        public long TelegramUserId
        {
            get { return telegramUserId; }
        }
        public string TelegramUserName
        {
            get { return telegramUserName; }
        }
        public DateTime RegisteredAt
        {
            get { return _registeredAt; }
        }
    }
}
