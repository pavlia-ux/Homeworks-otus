using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LinqToDB.Mapping;

namespace Homeworks_otus.TelegramBot.Core.DataAccess.Models
{
    [LinqToDB.Mapping.Table("ToDoUser")]
    public class ToDoUserModel
    {
        [PrimaryKey, Identity]
        [LinqToDB.Mapping.Column("id")]
        public int Id { get; set; }

        [LinqToDB.Mapping.Column("TelegramUserId"), LinqToDB.Mapping.NotNull]
        public long TelegramUserId { get; set; }

        [LinqToDB.Mapping.Column("TelegramUserName"), LinqToDB.Mapping.NotNull]
        public string TelegramUserName { get; set; }

        [LinqToDB.Mapping.Column("RegisteredAt")]
        public DateTime RegisteredAt { get; set; }
    }
}
