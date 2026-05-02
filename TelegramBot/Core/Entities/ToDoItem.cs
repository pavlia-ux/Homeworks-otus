using Homeworks_otus.TelegramBot.Core.Entities;

namespace Homeworks_otus.Core.Entities
{
    public class ToDoItem
    {
        public enum ToDoItemState
        {
            Active = 0, Completed = 1
        }

        public Guid Id { get; set; }
        public int? DatabaseId { get; set; }
        public Guid UserId { get; set; }
        public int? UserDatabaseId { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime DeadLine { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public int? ToDoListDatabaseId { get; set; }
        public ToDoList? ToDoList { get; set; }
    }
}
