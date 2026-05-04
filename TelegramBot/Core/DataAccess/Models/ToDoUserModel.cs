using LinqToDB.Mapping;

namespace Homeworks_otus.TelegramBot.Core.DataAccess.Models
{
    [Table("ToDoUser")]
    public class ToDoUserModel
    {
        [PrimaryKey, Identity]
        [Column("id")]
        public int Id { get; set; }

        [Column("ForeignId")]
        public Guid ForeignId { get; set; }

        [Column("TelegramUserId"), NotNull]
        public long TelegramUserId { get; set; }

        [Column("TelegramUserName"), NotNull]
        public string TelegramUserName { get; set; }

        [Column("RegisteredAt")]
        public DateTime RegisteredAt { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(ToDoListModel.UserId))]
        public List<ToDoListModel> ToDoLists { get; set; }

        [Association(ThisKey = nameof(Id), OtherKey = nameof(ToDoItemModel.UserId))]
        public List<ToDoItemModel> ToDoItems { get; set; }
    }
}
