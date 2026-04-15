using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;
using Homeworks_otus.TelegramBot.Core.Keyboard;
using Homeworks_otus.TelegramBot.Dto;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using static Homeworks_otus.TelegramBot.Scenarios.ScenarioResultClass;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioTypeClass;

namespace Homeworks_otus.TelegramBot.Core.Services
{
    internal class AddTaskScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoService _toDoService;
        public AddTaskScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    context.CurrentStep = "Name";
                    await botClient.SendMessage(message.Chat, "Введите название задачи", replyMarkup: ReplyKeyboard.SetCancelButton(), cancellationToken: ct);
                    return ScenarioResult.Transition;
                case "Name":
                    context.Data.Add("Name", message.Text);
                    context.CurrentStep = "DeadLine";
                    await botClient.SendMessage(message.Chat, $"Введите дату дедлайна Формат должен быть \"{DateTime.Now.ToShortDateString()}\"", replyMarkup: ReplyKeyboard.SetCancelButton(), cancellationToken: ct);
                    return ScenarioResult.Transition;
                case "DeadLine":
                    if (!DateTime.TryParse(message.Text, new CultureInfo("ru-RU"), out DateTime resultDL))
                    {
                        await botClient.SendMessage(message.Chat, $"Не верно введённая дата. Формат должен быть \"{DateTime.Now.ToShortDateString()}\" Попробуйте снова", replyMarkup: ReplyKeyboard.SetCancelButton(), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    if (resultDL < DateTime.Now)
                    {
                        await botClient.SendMessage(message.Chat, "Дата дедлайна не может быть меньше или равна текущей даты. Попробуйте снова", replyMarkup: ReplyKeyboard.SetCancelButton(), cancellationToken: ct);
                        return ScenarioResult.Transition;
                    }
                    context.Data.Add("DeadLine", message.Text);
                    List<InlineKeyboardButton[]> listButtons = new List<InlineKeyboardButton[]>();
                    IReadOnlyList<ToDoList> lists = await _toDoListService.GetUserLists((await _userService.GetUserByTelegramUserIdAsync(message.From.Id, ct)).UserId, ct);
                    context.Data["Lists"] = lists;
                    foreach (ToDoList list in lists)
                    {
                        listButtons.Add(new[] { new InlineKeyboardButton() { Text = list.Name, CallbackData = ToDoListCallbackDto.FromString($"SelectedList|{list.Id}").ToString() } });
                    }
                    await botClient.SendMessage(message.Chat, "Выберите список, в которой нужно добавить задачу:", replyMarkup: new InlineKeyboardMarkup(listButtons), cancellationToken: ct);
                    context.CurrentStep = "SelectedList";
                    return ScenarioResult.Transition;
                case "SelectedList":
                    ToDoList selectedList = null;
                    foreach (ToDoList list in (IReadOnlyList<ToDoList>)context.Data["Lists"])
                    {
                        if (list.Id.ToString() == context.Data["Callback"].ToString().Split("|")[1])
                        {
                            selectedList = list;
                            break;
                        }
                    }
                    await _toDoService.AddAsync(await _userService.GetUserByTelegramUserIdAsync(message.From.Id, ct), context.Data["Name"].ToString(), Convert.ToDateTime(context.Data["DeadLine"].ToString()), selectedList, ct);
                    break;
            }
            await botClient.SendMessage(message.Chat, "Задача успешно добавлена", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
            return ScenarioResult.Completed;
        }
    }
}
