using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;

using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.Core.Services
{
    public class UpdateHandler : IUpdateHandler
    {
        public static ToDoUser user = null;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }
        public void SetMaxLengthLimit(string? str)
        {
            _toDoService.MaxLength = ParseAndValidateInt(str, "MaxLength");
        }
        public void SetMaxQuantityLimit(string? str)
        {
            _toDoService.MaxQuantity = ParseAndValidateInt(str, "MaxQuantity");
        }

        int ParseAndValidateInt(string? str, string max)
        {
            if (max == "MaxLength")
            {
                if (!(int.TryParse(str, out int number) && number >= 1 && number <= _toDoService.MaxLength)) throw new ArgumentException($"Длина задачи не должна превышать {_toDoService.MaxLength} и быть меньше 1");
                return number;
            }
            else 
            {
                if (!(int.TryParse(str, out int number) && number >= 1 && number <= _toDoService.MaxQuantity)) throw new ArgumentException($"Количество задач не должно превышать {_toDoService.MaxQuantity} и быть меньше 1");
                return number;
            }
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                string inpCmd = update.Message.Text;

                do
                {
                    if (inpCmd.Equals("/start"))
                    {
                        botClient.SendMessage(update.Message.Chat, $"{Start(botClient, update)}");
                    }
                    else if (inpCmd.Equals("/help"))
                    {
                        botClient.SendMessage(update.Message.Chat, $"{Help()}");
                    }
                    else if (inpCmd.Equals("/info"))
                    {
                        botClient.SendMessage(update.Message.Chat, $"{Info()}");
                    }
                    else if (inpCmd.Contains("/addtask") && _userService.GetUser(update.Message.From.Id) != null)
                    {
                        _toDoService.Add(_userService.GetUser(update.Message.From.Id), update.Message.Text.Replace("/addtask", "").Trim());
                        botClient.SendMessage(update.Message.Chat, $"Задача добавлена");
                    }
                    else if (inpCmd.Equals("/showtasks") && _userService.GetUser(update.Message.From.Id) != null)
                    {
                        ShowTasks(botClient, update);
                    }
                    else if (inpCmd.Contains("/removetask") && _userService.GetUser(update.Message.From.Id) != null)
                    {
                        RemoveTask(inpCmd.Substring(12), botClient, update);
                    }
                    else if (inpCmd.Contains("/completetask") && _userService.GetUser(update.Message.From.Id) != null)
                    {
                        CompleteTask(inpCmd.Substring(14), botClient, update);
                    }
                    else if (inpCmd.Equals("/showalltasks") && _userService.GetUser(update.Message.From.Id) != null)
                    {
                        ShowAllTasks(botClient, update);
                    }
                    else if (inpCmd.Equals("/exit"))
                    {
                        Environment.Exit(1);
                    }
                    else
                    {
                        botClient.SendMessage(update.Message.Chat, "простите, но пока что я Вас не поняла :(");
                        break;
                    }
                } 
                while (string.IsNullOrWhiteSpace(inpCmd));
            }
            catch (TaskCountLimitException taskCountEx)
            {
                botClient.SendMessage(update.Message.Chat, taskCountEx.Message);
            }
            catch (TaskLengthLimitException taskLengthEx)
            {
                botClient.SendMessage(update.Message.Chat, taskLengthEx.Message);
            }
            catch (DuplicateTaskException duplicateTaskEx)
            {
                botClient.SendMessage(update.Message.Chat, duplicateTaskEx.Message);
            }
            catch (ArgumentException argEx)
            {
                botClient.SendMessage(update.Message.Chat, argEx.Message);
            }
            catch (Exception ex)
            {
                botClient.SendMessage(update.Message.Chat, $"Произошла непредвиденная ошибка: {ex.GetType().FullName} | {ex.Message} | {ex.StackTrace} | {ex.InnerException}");
            }
        }

        public string Start(ITelegramBotClient botClient, Update update)
        {
            ToDoUser? User = _userService.GetUser(update.Message.From.Id);
            if (User == null)
            {
                User = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
                return $"{User.TelegramUserName}, теперь Вы зарегистрированы! Вам доступны команды /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /exit";
            }
            else
            {
                return $"{User.TelegramUserName}, добрый день! Вам доступны команды / help, / info, / addtask, / showtasks, / removetask, / completetask, / showalltasks, / exit";
            }
        }
        public string Help()
        {
            string strHelp = "в этой программе следующий список доступных команд: /start, /help, " +
                        "/info, /exit.\r\n" +
                        "/start - программа просит Вас ввести своё имя, также сохраняет Ваш Id и дату регистрации.\r\n" +
                        "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                        "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                        "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                        "/showtasks - отображает список всех добавленных задач со статусом Active.\r\n" +
                        "/removetask - позволяет удалять задачи по номеру в общем списке.\r\n" +
                        "/completetask - позволяет ставить отметку о выполнении задачи по ее Id.\r\n" +
                        "/showalltasks - отображает список всех добавленных задач.\r\n" +
                        "/exit - программа заканчивает свою работу.";
            return strHelp;
        }
        public string Info()
        {
            string strInfo = "программа v4 создана 20.01.2026";
            return strInfo;
        }
        public void ShowTasks(ITelegramBotClient botClient, Update update)
        {
            Guid guidUserId = _userService.GetUser(update.Message.From.Id).UserId;
            var activeToDoItems = _toDoService.GetActiveByUserId(guidUserId);

            if (activeToDoItems.Count > 0)
            {
                int a = 1;
                for (int i = 0; i < activeToDoItems.Count; i++)
                {
                    botClient.SendMessage(update.Message.Chat, $"{a}. {activeToDoItems[i].Name} - {activeToDoItems[i].CreatedAt} - {activeToDoItems[i].Id}");
                    a++;
                }
            }

            else
            {
                botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст");
            }
        }
        public void RemoveTask(string taskId, ITelegramBotClient botClient, Update update)
        {
            Guid.TryParse(taskId, out Guid id);
            _toDoService.Delete(id);
            botClient.SendMessage(update.Message.Chat, "Ваша задача удалена");
        }
        public void CompleteTask(string taskId, ITelegramBotClient botClient, Update update)
        {
            Guid id = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(taskId)) Guid.TryParse(taskId, out id);
            _toDoService.MarkAsCompleted(id);
            botClient.SendMessage(update.Message.Chat, $"Задача отмечена как выполненная");
        }
        public void ShowAllTasks(ITelegramBotClient botClient, Update update)
        {
            Guid guidUserId = _userService.GetUser(update.Message.From.Id).UserId;
            var toDoItems = _toDoService.GetAllByUserId(guidUserId);

            if (toDoItems.Count > 0)
            {
                for (int i = 0; i < toDoItems.Count; i++)
                {
                    if (toDoItems[i].State == ToDoItemState.Completed)
                    {
                        botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems[i].State} - {toDoItems[i].StateChangedAt}| {toDoItems[i].Name} - {toDoItems[i].CreatedAt} - {toDoItems[i].Id}");
                    }
                    else
                    {
                        botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems[i].State}| {toDoItems[i].Name} - {toDoItems[i].CreatedAt} - {toDoItems[i].Id}");
                    }
                }
            }

            else
            {
                botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст");
            }
        }
    }
}
