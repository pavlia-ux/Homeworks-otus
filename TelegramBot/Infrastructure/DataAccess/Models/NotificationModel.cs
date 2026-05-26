using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess.Models;

using LinqToDB.Mapping;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess.Models
{
    [Table("Notification")]
    public class NotificationModel
    {
        [PrimaryKey, Identity]
        [Column("id")]
        public int Id { get; set; }

        [Column("ForeignId")]
        public Guid ForeignId { get; set; }

        [Column("UserId"), NotNull]
        public int UserId { get; set; }

        [Column("Type"), NotNull]
        public string Type { get; set; }

        [Column("Text"), NotNull]
        public string Text { get; set; }

        [Column("ScheduledAt"), NotNull]
        public DateTime ScheduledAt { get; set; }

        [Column("Is_Notified"), NotNull]
        public bool IsNotified { get; set; }

        [Column("NotifiedAt")]
        public DateTime? NotifiedAt { get; set; }

        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.Id))]
        public ToDoUserModel User { get; set; }
    }

}
