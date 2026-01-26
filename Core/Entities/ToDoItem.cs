using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.Core.Entities
{
    public class ToDoItem
    {
        public enum ToDoItemState
        {
            Active, Completed
        }

        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public ToDoItem(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
        }
    }
}
