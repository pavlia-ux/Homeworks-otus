using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.TelegramBot.Core.Entities
{
    public class ToDoList
    {
        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        public ToDoList(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            Name = name;
            User = user;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
