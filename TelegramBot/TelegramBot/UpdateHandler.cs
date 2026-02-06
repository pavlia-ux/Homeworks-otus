using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using Homeworks_otus.Core.DataAccess;
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
        private readonly IToDoReportService _toDoReportService;
        public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoReportService = toDoReportService;
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

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            try
            {
                string inpCmd = update.Message.Text;

                do
                {
                    if (inpCmd.Equals("/start"))
                    {
                        await botClient.SendMessage(update.Message.Chat, $"{Start(botClient, update, ct)}", ct);
                    }
                    else if (inpCmd.Equals("/help"))
                    {
                        await botClient.SendMessage(update.Message.Chat, $"{Help()}", ct);
                    }
                    else if (inpCmd.Equals("/info"))
                    {
                        await botClient.SendMessage(update.Message.Chat, $"{Info()}", ct);
                    }
                    else if (inpCmd.Contains("/addtask") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        _toDoService.AddAsync(_userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result, update.Message.Text.Replace("/addtask", "").Trim(), ct);
                        await botClient.SendMessage(update.Message.Chat, $"Задача добавлена", ct);
                    }
                    else if (inpCmd.Equals("/showtasks") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        ShowTasks(botClient, update, ct);
                    }
                    else if (inpCmd.Contains("/removetask") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        RemoveTask(inpCmd.Substring(12), botClient, update, ct);
                    }
                    else if (inpCmd.Contains("/completetask") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        CompleteTask(inpCmd.Substring(14), botClient, update, ct);
                    }
                    else if (inpCmd.Equals("/showalltasks") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        ShowAllTasks(botClient, update, ct);
                    }
                    else if (inpCmd.Equals("/report") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        Report(update.Message.From.Id, botClient, update, ct);
                    }
                    else if (inpCmd.Contains("/find") && _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct) != null)
                    {
                        Find(update.Message.From.Id, inpCmd.Substring(6), botClient, update, ct);
                    }
                    else if (inpCmd.Equals("/exit"))
                    {
                        Environment.Exit(1);
                    }
                    else
                    {
                        await botClient.SendMessage(update.Message.Chat, "простите, но пока что я Вас не поняла :(", ct);
                        break;
                    }
                }
                while (string.IsNullOrWhiteSpace(inpCmd));
            }
            catch (TaskCountLimitException taskCountEx)
            {
                HandleErrorAsync(botClient, taskCountEx, ct);
            }
            catch (TaskLengthLimitException taskLengthEx)
            {
                HandleErrorAsync(botClient, taskLengthEx, ct);
            }
            catch (DuplicateTaskException duplicateTaskEx)
            {
                HandleErrorAsync(botClient, duplicateTaskEx, ct);
            }
            catch (ArgumentException argEx)
            {
                HandleErrorAsync(botClient, argEx, ct);
            }
            catch (Exception ex)
            {
                HandleErrorAsync(botClient, ex, ct);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception.Message})");
        }

        public string Start(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? User = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result;
            if (User == null)
            {
                User = _userService.RegisterUserAsync(update.Message.From.Id, update.Message.From.Username, ct).Result;
                return $"{User.TelegramUserName}, теперь Вы зарегистрированы! Вам доступны команды /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /report, /find, /exit";
            }
            else
            {
                return $"{User.TelegramUserName}, добрый день! Вам доступны команды /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /report, /find, /exit";
            }
        }
        public string Help()
        {
            return "в этой программе следующий список доступных команд: /start, /help, " +
                   "/info, /exit.\r\n" +
                   "/start - программа просит Вас ввести своё имя, также сохраняет Ваш Id и дату регистрации.\r\n" +
                   "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                   "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                   "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                   "/showtasks - отображает список всех добавленных задач со статусом Active.\r\n" +
                   "/removetask - позволяет удалять задачи по номеру в общем списке.\r\n" +
                   "/completetask - позволяет ставить отметку о выполнении задачи по ее Id.\r\n" +
                   "/showalltasks - отображает список всех добавленных задач.\r\n" +
                   "/report - выводит завершенные/активные задачи на текущий момент.\r\n" +
                   "/find - отображает список всех добавленных задач.\r\n" +
                   "/exit - отображает список задач пользователя, которые начинаются на введенный префикс.";
        }
        public string Info()
        {
            return "программа v4 создана 20.01.2026";
        }
        public void ShowTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid guidUserId = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result.UserId;
            var activeToDoItems = _toDoService.GetActiveByUserIdAsync(guidUserId, ct);

            if (activeToDoItems.Result.Count > 0)
            {
                int a = 1;
                for (int i = 0; i < activeToDoItems.Result.Count; i++)
                {
                    botClient.SendMessage(update.Message.Chat, $"{a}. {activeToDoItems.Result[i].Name} - {activeToDoItems.Result[i].CreatedAt} - {activeToDoItems.Result[i].Id}", ct);
                    a++;
                }
            }

            else
            {
                botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст", ct);
            }
        }
        public void RemoveTask(string taskId, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid.TryParse(taskId, out Guid id);
            _toDoService.DeleteAsync(id, ct);
            botClient.SendMessage(update.Message.Chat, "Ваша задача удалена", ct);
        }
        public void CompleteTask(string taskId, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid id = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(taskId)) Guid.TryParse(taskId, out id);
            _toDoService.MarkAsCompletedAsync(id, ct);
            botClient.SendMessage(update.Message.Chat, $"Задача отмечена как выполненная", ct);
        }
        public void ShowAllTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid guidUserId = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result.UserId;
            var toDoItems = _toDoService.GetAllByUserIdAsync(guidUserId, ct);

            if (toDoItems.Result.Count > 0)
            {
                for (int i = 0; i < toDoItems.Result.Count; i++)
                {
                    if (toDoItems.Result[i].State == ToDoItemState.Completed)
                    {
                        botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems.Result[i].State} - {toDoItems.Result[i].StateChangedAt}| {toDoItems.Result[i].Name} - {toDoItems.Result[i].CreatedAt} - {toDoItems.Result[i].Id}", ct);
                    }
                    else
                    {
                        botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems.Result[i].State}| {toDoItems.Result[i].Name} - {toDoItems.Result[i].CreatedAt} - {toDoItems.Result[i].Id}", ct);
                    }
                }
            }

            else
            {
                botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст", ct);
            }
        }
        public void Report(long id, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? user = _userService.GetUserByTelegramUserIdAsync(id, ct).Result;
            var userStats = _toDoReportService.GetUserStatsAsync(user.UserId, ct);
            botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {userStats.Result.generatedAt}. Всего: {userStats.Result.total}; Завершенных: {userStats.Result.completed}; Активных: {userStats.Result.active};", ct);
        }
        public void Find(long id, string predicate, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? user = _userService.GetUserByTelegramUserIdAsync(id, ct).Result;
            var toDoItems = _toDoService.FindAsync(user, predicate, ct);
            
            for (int i = 0; i < toDoItems.Result.Count; i++)
            {
                botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems.Result[i].State}| {toDoItems.Result[i].Name} - {toDoItems.Result[i].CreatedAt} - {toDoItems.Result[i].Id}", ct);
            }
        }
    }
}
