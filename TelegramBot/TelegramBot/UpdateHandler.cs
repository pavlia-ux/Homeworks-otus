using System;
using System.Collections;
using System.Collections.Generic;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.Core.Exceptions;
using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;
using Homeworks_otus.TelegramBot.Core.Keyboard;
using Homeworks_otus.TelegramBot.Core.Services;
using Homeworks_otus.TelegramBot.Dto;
using Homeworks_otus.TelegramBot.Extensions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Homeworks_otus.Core.Entities.ToDoItem;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioResultClass;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioTypeClass;

namespace Homeworks_otus.Core.Services
{
    public class UpdateHandler : IUpdateHandler
    {
        public static ToDoUser user = null;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        private readonly IToDoReportService _toDoReportService;
        private readonly IToDoListService _toDoListService;
        private readonly IEnumerable _scenarios;
        private readonly IScenarioContextRepository _scenarioContextRepository;
        private static int _pageSize = 5;
        public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService, IToDoListService toDoListService, IEnumerable scenarios, IScenarioContextRepository scenarioContextRepository)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoReportService = toDoReportService;
            _toDoListService = toDoListService;
            _scenarios = scenarios;
            _scenarioContextRepository = scenarioContextRepository;
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
                switch (update.Type)
                {
                    case UpdateType.CallbackQuery:
                        if (await IsRegistered(botClient, update.CallbackQuery.Message, ct))
                            await HandleCallBack(botClient, update, ct);
                        break;
                    case UpdateType.Message:
                        await HandleMessage(botClient, update, ct);
                        break;
                    default:
                        await botClient.SendMessage(update.Message.Chat, "Такой формат пока не поддерживается!", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                        return;
                }
            }
            catch (TaskCountLimitException taskCountEx)
            {
                await HandleErrorAsync(botClient, taskCountEx, HandleErrorSource.HandleUpdateError, ct);
                await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
            }
            catch (TaskLengthLimitException taskLengthEx)
            {
                await HandleErrorAsync(botClient, taskLengthEx, HandleErrorSource.HandleUpdateError, ct);
                await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
            }
            catch (DuplicateTaskException duplicateTaskEx)
            {
                await HandleErrorAsync(botClient, duplicateTaskEx, HandleErrorSource.HandleUpdateError, ct);
                await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
            }
            catch (ArgumentException argEx)
            {
                await HandleErrorAsync(botClient, argEx, HandleErrorSource.HandleUpdateError, ct);
                await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, ct);
                await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
            }
        }

        private async Task HandleMessage(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ScenarioContext? context;
            if (update.Message.Text.StartsWith("/cancel"))
            {
                await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
                await botClient.SendMessage(update.Message.Chat, "Сценарий отменён.", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                return;
            }
            context = await _scenarioContextRepository.GetContext(update.Message.From.Id, ct);
            if (context != null)
            {
                await ProcessScenario(botClient, context, update.Message.From, update.Message, ct);
                return;
            }
            switch (update.Message.Text)
            {
                case "/start":
                    await Start(botClient, update, ct);
                    await Help(botClient, update, ct);
                    break;
                case "/help":
                    await Help(botClient, update, ct);
                    break;
                case "/info":
                    await Info(botClient, update, ct);
                    break;
                case "/addtask":
                    if (await IsRegistered(botClient, update.Message, ct))
                    {
                        context = new ScenarioContext(ScenarioType.AddTask);
                        await _scenarioContextRepository.SetContext(update.Message.From.Id, context, ct);
                        await ProcessScenario(botClient, context, update.Message.From, update.Message, ct);
                    }
                    break;
                case "/show":
                    if (await IsRegistered(botClient, update.Message, ct))
                    {
                        await Show(botClient, update, ct);
                    }
                    break;
                case "/report":
                    if (await IsRegistered(botClient, update.Message, ct))
                    {
                        IToDoReportService report = new ToDoReportService(_toDoService);
                        var stat = (await report.GetUserStatsAsync((await _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct)).UserId, ct));
                        await botClient.SendMessage(update.Message.Chat, $"Статистика по задачами на {stat.generatedAt}. Всего: {stat.total}; Завершённых: {stat.completed}; Активных: {stat.active}.", cancellationToken: ct);
                    }
                    break;
                case string a when a.IndexOf("/find") == 0:
                    if (await IsRegistered(botClient, update.Message, ct))
                    {
                        await botClient.SendMessage(update.Message.Chat, await FindTasks(update, a.Replace("/find", "").Trim(), ct), cancellationToken: ct);
                    }
                    break;
                default:
                    await botClient.SendMessage(update.Message.Chat, "Такой команды не существует!", cancellationToken: ct);
                    break;
            }
        }

        private async Task HandleCallBack(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ScenarioContext? context = await _scenarioContextRepository.GetContext(update.CallbackQuery.From.Id, ct);
            if (context != null)
            {
                context.Data["Callback"] = update.CallbackQuery.Data;
                await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                return;
            }
            switch (update.CallbackQuery)
            {
                case CallbackQuery a when a.Data == "addlist":
                    context = new ScenarioContext(ScenarioType.AddList);
                    await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, context, ct);
                    await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                    break;
                case CallbackQuery a when a.Data == "deletelist":
                    context = new ScenarioContext(ScenarioType.DeleteList);
                    await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, context, ct);
                    await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                    break;
                case CallbackQuery a when a.Data.StartsWith("show"):
                    if (a.Data.StartsWith("showtask"))
                    {
                        ToDoItemCallbackDto itemDTO = ToDoItemCallbackDto.FromString(a.Data);
                        ToDoItem task = await _toDoService.Get(itemDTO.ToDoItemId, ct);
                        string answer = string.Empty;
                        InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup();
                        if (task.State == ToDoItemState.Active)
                        {
                            answer = $"{task.Name}\r\nСрок выполнения:{task.DeadLine}\r\nВремя выполнения:{task.CreatedAt}";
                            keyboardMarkup.AddNewRow(new InlineKeyboardButton[]
                            {
                            new InlineKeyboardButton("✅Выполнить",ToDoItemCallbackDto.FromString($"completetask|{itemDTO.ToDoItemId}").ToString()),
                            new InlineKeyboardButton("❌Удалить",ToDoItemCallbackDto.FromString($"deletetask|{itemDTO.ToDoItemId}").ToString())
                            });
                        }
                        else
                            answer = $"{task.Name}\r\nСрок выполнения:{task.DeadLine}\r\nВремя выполнения:{task.CreatedAt}\r\nВремя завершения:{task.StateChangedAt}";
                        await botClient.SendMessage(update.CallbackQuery.Message.Chat, answer, replyMarkup: keyboardMarkup, cancellationToken: ct);
                        break;
                    }
                    PagedListCallbackDto listDTO = PagedListCallbackDto.FromString(a.Data);
                    IReadOnlyList<ToDoItem> tasks = null;
                    Guid userId = (await _userService.GetUserByTelegramUserIdAsync(update.CallbackQuery.Message.From.Id, ct)).UserId;
                    if (listDTO.Action == "show" && listDTO.ToDoListId != null)//получаем список активных задач с привязкой к списку.
                        tasks = (await _toDoService.GetByUserIdAndList(userId, listDTO.ToDoListId, ct)).Where(task => task.State == ToDoItemState.Active).ToList();
                    else if (listDTO.Action == "show")//список активных задач без привязки к списку.
                        tasks = await _toDoService.GetActiveByUserIdAsync(userId, ct);
                    else if (listDTO.Action == $"show_completed" && listDTO.ToDoListId != null)//список завершённых задач с привязкой к списку.
                        tasks = (await _toDoService.GetByUserIdAndList(userId, listDTO.ToDoListId, ct)).Where(task => task.State == ToDoItemState.Completed).ToList();
                    else if (listDTO.Action == $"show_completed")//список завершённых задач без привязки к списку.
                        tasks = await _toDoService.GetCompletedByUserIdAsync(userId, ct);
                    List<KeyValuePair<string, string>> callbackData = new List<KeyValuePair<string, string>>();
                    foreach (ToDoItem task in tasks)
                    {
                        callbackData.Add(new KeyValuePair<string, string>(task.Name, ToDoItemCallbackDto.FromString($"showtask|{task.Id}").ToString()));
                    }

                    if (tasks.Count == 0)
                        await botClient.SendMessage(update.CallbackQuery.Message.Chat, (listDTO.ToDoListId != null ? "Задач в списке нет" : "Задачи отсутствуют"), cancellationToken: ct);
                    else
                        await botClient.EditMessageText(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, "Активные задачи", replyMarkup: BuildPagedButtons(callbackData, listDTO), cancellationToken: ct);
                    break;
                case CallbackQuery a when a.Data.StartsWith("completetask"):
                    ToDoItemCallbackDto tdo = ToDoItemCallbackDto.FromString(a.Data);
                    await _toDoService.MarkAsCompletedAsync(tdo.ToDoItemId, ct);
                    await botClient.EditMessageReplyMarkup(update.CallbackQuery.Message.Chat.Id, update.CallbackQuery.Message.MessageId, null, cancellationToken: ct);
                    await botClient.SendMessage(update.CallbackQuery.Message.Chat, "Задача завершена", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                    break;
                case CallbackQuery a when a.Data.StartsWith("deletetask"):
                    context = new ScenarioContext(ScenarioType.DeleteTask);
                    context.Data.Add("Callback", ToDoItemCallbackDto.FromString(a.Data).ToString());
                    await _scenarioContextRepository.SetContext(update.CallbackQuery.From.Id, context, ct);
                    await ProcessScenario(botClient, context, update.CallbackQuery.From, update.CallbackQuery.Message, ct);
                    ToDoItemCallbackDto tdo2 = ToDoItemCallbackDto.FromString(a.Data);
                    break;
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception.Message})");
        }
    
        private InlineKeyboardMarkup BuildPagedButtons(IReadOnlyList<KeyValuePair<string, string>> callbackData, PagedListCallbackDto listDto)
        {
            InlineKeyboardMarkup keyboardMarkup = new InlineKeyboardMarkup();
            int allCount = callbackData.Count;
            int totalPage = (int)Math.Round((decimal)callbackData.Count / _pageSize, MidpointRounding.ToPositiveInfinity);//расчёт количества страниц.
            callbackData = callbackData.GetBatchByNumber(_pageSize, listDto.Page).ToList();//берём только те элементы, где страница равна той, которая указана во втором параметре.
            foreach (KeyValuePair<string, string> keyVal in callbackData)
            {
                keyboardMarkup.AddNewRow(new InlineKeyboardButton(keyVal.Key, keyVal.Value));
            }
            if (allCount > _pageSize)
            {
                if (listDto.Page == 0)
                {//настраиваем кнопки перехода по страницам
                    keyboardMarkup.AddNewRow(new InlineKeyboardButton("➡️", PagedListCallbackDto.FromString($"show|{listDto.ToDoListId}|{listDto.Page + 1}").ToString()));
                }
                else if (listDto.Page > 0 && listDto.Page < totalPage - 1)
                {
                    keyboardMarkup.AddNewRow(new InlineKeyboardButton[]
                    {
                    new InlineKeyboardButton("⬅️",PagedListCallbackDto.FromString($"show|{listDto.ToDoListId}|{listDto.Page - 1}").ToString()),
                    new InlineKeyboardButton("➡️",PagedListCallbackDto.FromString($"show|{listDto.ToDoListId}|{listDto.Page + 1}").ToString())
                    });
                }
                else
                {
                    keyboardMarkup.AddNewRow(new InlineKeyboardButton("⬅️", PagedListCallbackDto.FromString($"show|{listDto.ToDoListId}|{listDto.Page - 1}").ToString()));
                }
            }
            if (listDto.Action != "show_completed")
                keyboardMarkup.AddNewRow(new InlineKeyboardButton("Посмотреть выполненные", PagedListCallbackDto.FromString($"show_completed|{listDto.ToDoListId}|0").ToString()));
            return keyboardMarkup;
        }

        private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, User user, Message msg, CancellationToken ct)
        {
            IScenario scenario = GetScenario(context.CurrentScenario);
            if (await scenario.HandleMessageAsync(botClient, context, msg, ct) == ScenarioResult.Completed)
                await _scenarioContextRepository.ResetContext(user.Id, ct);
        }

        private IScenario GetScenario(ScenarioType scenarioType)
        {
            foreach (IScenario scenario in _scenarios)
            {
                if (scenario.CanHandle(scenarioType))
                {
                    return scenario;
                }
            }
            throw new ArgumentException("Сценарий не найден");
        }

        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? User = await _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct);
            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(new List<KeyboardButton>
                {
                    new KeyboardButton("/show"),
                    new KeyboardButton("/report")
                });
            if (User != null)
            {
                await botClient.SendMessage(update.Message.Chat, $"{User.TelegramUserName}, команда уже выполнена.", replyMarkup: keyboard, cancellationToken: ct);
            }
            else
            {
                User = await _userService.RegisterUserAsync(update.Message.From.Id, update.Message.From.Username ?? "", ct);
                await botClient.SendMessage(update.Message.Chat, $"{User.TelegramUserName}, добро пожаловать!", replyMarkup: keyboard, cancellationToken: ct);
            }
        }
        public async Task Help(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await botClient.SendMessage(update.Message.Chat, "в этой программе следующий список доступных команд:\r\n" +
                   "/start - программа просит Вас ввести своё имя, также сохраняет Ваш Id и дату регистрации.\r\n" +
                   "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                   "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                   "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                   "/show - отображает список всех добавленных задач со статусом Active.\r\n" +
                   "/report - выводит завершенные/активные задачи на текущий момент.\r\n" +
                   "/find - отображает список задач пользователя, которые начинаются на введенный префикс.\r\n" +
                   "/cancel - останавливает сценарии.", cancellationToken: ct);
        }
        public async Task Info(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await botClient.SendMessage(update.Message.Chat, "программа v4 создана 20.01.2026", cancellationToken: ct);
        }
        public async Task Show(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            List<InlineKeyboardButton[]> buttons = new List<InlineKeyboardButton[]>();
            buttons.Add(new[]
            {
                new InlineKeyboardButton()
                {
                    Text = "📌Без списка",
                    CallbackData = "show"
                }
            });
            IReadOnlyList<ToDoList> userLists = await _toDoListService.GetUserLists((await _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct)).UserId, ct);
            foreach (ToDoList list in userLists)
            {
                buttons.Add(new[] { new InlineKeyboardButton() { Text = list.Name, CallbackData = ToDoListCallbackDto.FromString($"show|{list.Id}").ToString() } });
            }
            buttons.Add(new[]
            {
                new InlineKeyboardButton()
                {Text = "🆕Добавить", CallbackData = "addlist"},
                new InlineKeyboardButton()
                {Text = "❌Удалить", CallbackData = "deletelist"}
            });
            await botClient.SendMessage(update.Message.Chat, "Выберите список", replyMarkup: new InlineKeyboardMarkup(buttons), cancellationToken: ct);
        }
        public async Task Report(long id, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? user = _userService.GetUserByTelegramUserIdAsync(id, ct).Result;
            var userStats = _toDoReportService.GetUserStatsAsync(user.UserId, ct);
            await botClient.SendMessage(update.Message.Chat, $"Статистика по задачам на {userStats.Result.generatedAt}. Всего: {userStats.Result.total}; Завершенных: {userStats.Result.completed}; Активных: {userStats.Result.active};", cancellationToken: ct);
        }
        private async Task<string> FindTasks(Update update, string namePrefix, CancellationToken ct)
        {
            Guid guid = (await _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct)).UserId;
            var tasks = await _toDoService.FindAsync(guid, namePrefix, ct);
            string result = string.Empty;
            int i = 1;
            foreach (ToDoItem Task in tasks)
            {
                result += $"{i++})ID:{Task.Id}, Название:{Task.Name}, Дата создания:{Task.CreatedAt}\r\n";
            }
            if (result == string.Empty)
                result = "Задач в списке нет.";
            return result;
        }
        private async Task<bool> IsRegistered(ITelegramBotClient bot, Message message, CancellationToken ct)
        {
            if (await _userService.GetUserByTelegramUserIdAsync(message.From.Id, ct) == null)
            {
                await bot.SendMessage(message.Chat, "Команда доступна только для зарегистрированных пользователей. /start Для запуска.", cancellationToken: ct);
                return false;
            }
            else
                return true;
        }
    }
}
