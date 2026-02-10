using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
                        await botClient.SendMessage(update.Message.Chat, $"{Start(botClient, update, ct)}", cancellationToken: ct);
                        await botClient.SendMessage(update.Message.Chat, "Нажмите клавишу F для выхода", cancellationToken: ct);
                        break;
                    }

                    if (_userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result != null)
                    {
                        //botClient.SendMessage(update.Message.Chat, "Введите максимально допустимую длину задачи:", cancellationToken: ct);
                        //SetMaxLengthLimit(Console.ReadLine());
                        //botClient.SendMessage(update.Message.Chat, "Введите максимально допустимое количество задач:", cancellationToken: ct);
                        //SetMaxQuantityLimit(Console.ReadLine());

                        if (inpCmd.Equals("/help"))
                        {
                            await botClient.SendMessage(update.Message.Chat, $"{Help(botClient, update, ct)}", cancellationToken: ct);
                            break;
                        }
                        else if (inpCmd.Equals("/info"))
                        {
                            await botClient.SendMessage(update.Message.Chat, $"{Info(botClient, update, ct)}", cancellationToken: ct);
                            break;
                        }
                        else if (inpCmd.Contains("/addtask"))
                        {
                            _toDoService.AddAsync(_userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result, update.Message.Text.Replace("/addtask", "").Trim(), ct);
                            await botClient.SendMessage(update.Message.Chat, "Задача добавлена", cancellationToken: ct);
                            break;
                        }
                        else if (inpCmd.Equals("/showtasks"))
                        {
                            await ShowTasks(botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Contains("/removetask"))
                        {
                            await RemoveTask(inpCmd.Substring(12), botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Contains("/completetask"))
                        {
                            await CompleteTask(inpCmd.Substring(14), botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Equals("/showalltasks"))
                        {
                            await ShowAllTasks(botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Equals("/report"))
                        {
                            await Report(update.Message.From.Id, botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Contains("/find"))
                        {
                            await Find(update.Message.From.Id, inpCmd.Substring(6), botClient, update, ct);
                            break;
                        }
                        else
                        {
                            await botClient.SendMessage(update.Message.Chat, "Простите, но пока что я Вас не поняла :(", cancellationToken: ct);
                            break;
                        }
                    }
                    else 
                    {
                        await botClient.SendMessage(update.Message.Chat, "Бот доступен после команды /start", cancellationToken: ct);
                        break;
                    }
                }
                while (string.IsNullOrWhiteSpace(inpCmd));
            }
            catch (TaskCountLimitException taskCountEx)
            {
                HandleErrorAsync(botClient, taskCountEx, HandleErrorSource.HandleUpdateError, ct);
            }
            catch (TaskLengthLimitException taskLengthEx)
            {
                HandleErrorAsync(botClient, taskLengthEx, HandleErrorSource.HandleUpdateError, ct);
            }
            catch (DuplicateTaskException duplicateTaskEx)
            {
                HandleErrorAsync(botClient, duplicateTaskEx, HandleErrorSource.HandleUpdateError, ct);
            }
            catch (ArgumentException argEx)
            {
                HandleErrorAsync(botClient, argEx, HandleErrorSource.HandleUpdateError, ct);
            }
            catch (Exception ex)
            {
                HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, ct);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception.Message})");
        }

        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? User = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result;

            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                    new KeyboardButton("/showalltasks"),
                    new KeyboardButton("/showtasks"),
                    new KeyboardButton("/report"),
                })
            {
                ResizeKeyboard = true
            };

            if (User == null)
            {
                User = _userService.RegisterUserAsync(update.Message.From.Id, update.Message.From.Username, ct).Result;
                await botClient.SendMessage(update.Message.Chat, $"{User.TelegramUserName}, теперь Вы зарегистрированы!", replyMarkup: replyKeyboardMarkup, cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(update.Message.Chat, $"{User.TelegramUserName}, добрый день!", replyMarkup: replyKeyboardMarkup, cancellationToken: ct);
            }
        }
        public async Task Help(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await botClient.SendMessage(update.Message.Chat, "в этой программе следующий список доступных команд:\r\n" +
                   "/start - программа просит Вас ввести своё имя, также сохраняет Ваш Id и дату регистрации.\r\n" +
                   "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                   "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                   "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                   "/showtasks - отображает список всех добавленных задач со статусом Active.\r\n" +
                   "/removetask - позволяет удалять задачи по Id в общем списке.\r\n" +
                   "/completetask - позволяет ставить отметку о выполнении задачи по ее Id.\r\n" +
                   "/showalltasks - отображает список всех добавленных задач.\r\n" +
                   "/report - выводит завершенные/активные задачи на текущий момент.\r\n" +
                   "/find - отображает список задач пользователя, которые начинаются на введенный префикс.", cancellationToken: ct);
        }
        public async Task Info(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await botClient.SendMessage(update.Message.Chat, "программа v4 создана 20.01.2026", cancellationToken: ct);
        }
        public async Task ShowTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid guidUserId = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result.UserId;
            var activeToDoItems = _toDoService.GetActiveByUserIdAsync(guidUserId, ct);

            if (activeToDoItems.Result.Count > 0)
            {
                int a = 1;
                for (int i = 0; i < activeToDoItems.Result.Count; i++)
                {
                    await botClient.SendMessage(update.Message.Chat, $"{a}. {activeToDoItems.Result[i].Name} - {activeToDoItems.Result[i].CreatedAt} - `{activeToDoItems.Result[i].Id}`", cancellationToken: ct);
                    a++;
                }
            }

            else
            {
                await botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст", cancellationToken: ct);
            }
        }
        public async Task RemoveTask(string taskId, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid.TryParse(taskId, out Guid id);
            _toDoService.DeleteAsync(id, ct);
            await botClient.SendMessage(update.Message.Chat, "Ваша задача удалена", cancellationToken: ct);
        }
        public async Task CompleteTask(string taskId, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid id = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(taskId)) Guid.TryParse(taskId, out id);
            _toDoService.MarkAsCompletedAsync(id, ct);
            await botClient.SendMessage(update.Message.Chat, $"Задача отмечена как выполненная", cancellationToken: ct);
        }
        public async Task ShowAllTasks(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            Guid guidUserId = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result.UserId;
            var toDoItems = _toDoService.GetAllByUserIdAsync(guidUserId, ct);

            if (toDoItems.Result.Count > 0)
            {
                for (int i = 0; i < toDoItems.Result.Count; i++)
                {
                    if (toDoItems.Result[i].State == ToDoItemState.Completed)
                    {
                        await botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems.Result[i].State} - {toDoItems.Result[i].StateChangedAt}| {toDoItems.Result[i].Name} - {toDoItems.Result[i].CreatedAt} - `{toDoItems.Result[i].Id}`", cancellationToken: ct);
                    }
                    else
                    {
                        await botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems.Result[i].State}| {toDoItems.Result[i].Name} - {toDoItems.Result[i].CreatedAt} - `{toDoItems.Result[i].Id}`", cancellationToken: ct);
                    }
                }
            }

            else
            {
                await botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст", cancellationToken: ct);
            }
        }
        public async Task Report(long id, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? user = _userService.GetUserByTelegramUserIdAsync(id, ct).Result;
            var userStats = _toDoReportService.GetUserStatsAsync(user.UserId, ct);
            await botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {userStats.Result.generatedAt}. Всего: {userStats.Result.total}; Завершенных: {userStats.Result.completed}; Активных: {userStats.Result.active};", cancellationToken: ct);
        }
        public async Task Find(long id, string predicate, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? user = _userService.GetUserByTelegramUserIdAsync(id, ct).Result;
            var toDoItems = _toDoService.FindAsync(user, predicate, ct);
            
            for (int i = 0; i < toDoItems.Result.Count; i++)
            {
                await botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems.Result[i].State}| {toDoItems.Result[i].Name} - {toDoItems.Result[i].CreatedAt} - {toDoItems.Result[i].Id}", cancellationToken: ct);
            }
        }
    }
}
