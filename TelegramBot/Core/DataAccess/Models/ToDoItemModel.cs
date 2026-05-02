using LinqToDB.Mapping;


namespace Homeworks_otus.TelegramBot.Core.DataAccess.Models
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey, Identity]
        [Column("id")]
        public int Id { get; set; }
               
        [Column("ForeignId")]
        public Guid ForeignId { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("ItemName"), NotNull]
        public string ItemName { get; set; }

        [Column("ItemCreatedAt")]
        public DateTime ItemCreatedAt { get; set; }

        [Column("DeadLine"), NotNull]
        public DateTime DeadLine { get; set; }

        [Column("StateChangedAt")]
        public DateTime StateChangedAt { get; set; }

        [Column("ToDoListId")]
        public int? ToDoListId { get; set; }

        [Column("ToDoItemState"), NotNull]
        public int ToDoItemState { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }

        [Association(ThisKey = nameof(ToDoListId), OtherKey = nameof(ToDoListModel.Id), CanBeNull = true)]
        public ToDoListModel ToDoList { get; set; }
    }
}
