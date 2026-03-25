using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.TelegramBot.Dto
{
    public class CallbackDto
    {
        public string Action { get; set; } //с помощью него будет определять за какое действие отвечает кнопка
        public static CallbackDto FromString(string input) //На вход принимает строку ввида "{action}|{prop1}|{prop2}...". Нужно создать CallbackDto с Action = action. Нужно учесть что в строке может не быть |, тогда всю строку сохраняем в Action.
        {
            string[] values = input.Split('|');
            CallbackDto dto = new CallbackDto();
            if (values.Length == 0)
                dto.Action = input;
            else
                dto.Action = values[0];
            return dto;
        }
        public override string ToString() => Action; //переопределить метод.Он должен возвращать Action
    }
}
