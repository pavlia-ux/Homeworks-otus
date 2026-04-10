using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.TelegramBot.Dto
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }
        public static new ToDoItemCallbackDto FromString(string input)
        {
            string[] values = input.Split('|');
            ToDoItemCallbackDto dto = new ToDoItemCallbackDto();
            dto.Action = values[0];
            dto.ToDoItemId = Guid.Parse(values[1]);
            return dto;
        }
        public override string ToString() => $"{base.ToString()}|{ToDoItemId}";
    }
}
