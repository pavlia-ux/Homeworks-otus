using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Telegram.Bot.Types.ReplyMarkups;

namespace Homeworks_otus.TelegramBot.Core.Keyboard
{
    internal class ReplyKeyboard
    {
        public static ReplyKeyboardMarkup SetReplyMarkupKeyboard(params string[] keyboardName)
        {
            List<KeyboardButton> buttons = new List<KeyboardButton>();
            foreach (string name in keyboardName)
            {
                buttons.Add(new KeyboardButton(name));
            }
            return new ReplyKeyboardMarkup(buttons);
        }
        public static ReplyKeyboardMarkup SetStandardListButton()
        {
            return new ReplyKeyboardMarkup(new List<KeyboardButton>()
            {
                new KeyboardButton("/addtask"),
                new KeyboardButton("/showtasks"),
                new KeyboardButton("/showalltasks") });
        }
        public static ReplyKeyboardMarkup SetCancelButton()
        {
            return new ReplyKeyboardMarkup(new List<KeyboardButton>
            {
                new KeyboardButton("/cancel")
            });
        }
    }
}
