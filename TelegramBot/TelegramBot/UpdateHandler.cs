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

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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
                ScenarioContext? context;
                string inpCmd = update.Message.Text;

                if (inpCmd.StartsWith("/cancel"))
                {
                    await _scenarioContextRepository.ResetContext(update.Message.From.Id, ct);
                    await botClient.SendMessage(update.Message.Chat, "Сценарий отменён.", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                    return;
                }
                context = await _scenarioContextRepository.GetContext(update.Message.From.Id, ct);
                if (context != null)
                {
                    await ProcessScenario(botClient, context, update.Message, ct);
                    return;
                }

                do
                {
                    if (inpCmd.Equals("/start"))
                    {
                        await Start(botClient, update, ct);
                        break;
                    }

                    if (_userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result != null)
                    {
                        if (inpCmd.Equals("/help"))
                        {
                            await Help(botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Equals("/info"))
                        {
                            await Info(botClient, update, ct);
                            break;
                        }
                        else if (inpCmd.Contains("/addtask"))
                        {
                            context = new ScenarioContext(ScenarioType.AddTask);
                            await _scenarioContextRepository.SetContext(update.Message.From.Id, context, ct);
                            await ProcessScenario(botClient, context, update.Message, ct);
                            break;
                        }
                        else if (inpCmd.StartsWith("/show"))
                        {
                            await Show(botClient, update, true, ct);
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

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken ct)
        {
            Console.WriteLine($"HandleError: {exception.Message})");
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
        private async Task ProcessScenario(ITelegramBotClient botClient, ScenarioContext context, Message msg, CancellationToken ct)
        {
            IScenario scenario = GetScenario(context.CurrentScenario);
            if (await scenario.HandleMessageAsync(botClient, context, msg, ct) == ScenarioResult.Completed)
                await _scenarioContextRepository.ResetContext(msg.From.Id, ct);
        }

        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            ToDoUser? User = _userService.GetUserByTelegramUserIdAsync(update.Message.From.Id, ct).Result;

            ReplyKeyboardMarkup replyKeyboardMarkup = new(
                new[]
                {
                    new KeyboardButton("/show"),
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
