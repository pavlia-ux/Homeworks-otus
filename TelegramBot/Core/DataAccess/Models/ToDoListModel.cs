using LinqToDB.Mapping;

namespace Homeworks_otus.TelegramBot.Core.DataAccess.Models
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey, Identity]
        [Column("id")]
        public int Id { get; set; }

        [Column("ForeignId")]
        public Guid ForeignId { get; set; }

        [Column("ListName"), NotNull]
        public string ListName { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("ListCreatedAt")]
        public DateTime ListCreatedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(ToDoItemModel.ToDoListId))]
        public List<ToDoItemModel> ToDoItems { get; set; }
    }
}
