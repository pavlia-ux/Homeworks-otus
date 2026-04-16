using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

using LinqToDB.Data;
using LinqToDB.Mapping;


namespace Homeworks_otus.TelegramBot.Core.DataAccess.Models
{
    [LinqToDB.Mapping.Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey, Identity]
        [LinqToDB.Mapping.Column("id")]
        public int Id { get; set; }

        [LinqToDB.Mapping.Column("UserId")]
        public int UserId { get; set; }

        [LinqToDB.Mapping.Column("ItemName"), LinqToDB.Mapping.NotNull]
        public string ItemName { get; set; }

        [LinqToDB.Mapping.Column("ItemCreatedAt")]
        public DateTime ItemCreatedAt { get; set; }

        [LinqToDB.Mapping.Column("DeadLine"), LinqToDB.Mapping.NotNull]
        public DateTime DeadLine { get; set; }

        [LinqToDB.Mapping.Column("StateChangedAt")]
        public DateTime StateChangedAt { get; set; }

        [LinqToDB.Mapping.Column("ListId")]
        public int? ListId { get; set; }

        [LinqToDB.Mapping.Column("ToDoItemState"), LinqToDB.Mapping.NotNull]
        public int ToDoItemState { get; set; }

        [LinqToDB.Mapping.Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public virtual ToDoUserModel User { get; set; }

        [LinqToDB.Mapping.Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id), CanBeNull = true)]
        public virtual ToDoListModel ToDoList { get; set; }
    }
}
