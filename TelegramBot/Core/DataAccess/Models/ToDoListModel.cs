using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Mapping;

using Homeworks_otus.Core.Entities;

namespace Homeworks_otus.TelegramBot.Core.DataAccess.Models
{
    [LinqToDB.Mapping.Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey, Identity]
        [LinqToDB.Mapping.Column("id")]
        public int Id { get; set; }

        [LinqToDB.Mapping.Column("ListName"), LinqToDB.Mapping.NotNull]
        public string ListName { get; set; }

        [LinqToDB.Mapping.Column("UserId")]
        public int UserId { get; set; }

        [LinqToDB.Mapping.Column("ListCreatedAt")]
        public DateTime ListCreatedAt { get; set; }
                
        [LinqToDB.Mapping.Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUser User { get; set; }
    }
}
