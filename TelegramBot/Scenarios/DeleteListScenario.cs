using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;
using Homeworks_otus.TelegramBot.Core.Keyboard;
using Homeworks_otus.TelegramBot.Core.Services;
using Homeworks_otus.TelegramBot.Infrastructure.DataAccess;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using static Homeworks_otus.TelegramBot.Scenarios.ScenarioResultClass;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioTypeClass;

namespace Homeworks_otus.TelegramBot.Scenarios
{
    public class DeleteListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        private readonly IToDoService _toDoService;
        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService) 
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.DeleteList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    await botClient.SendMessage(message.Chat, "Выберите список для удаления:", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    return ScenarioResult.Transition;
                case "Approve":
                    ToDoList selectedList = null;
                    foreach (ToDoList list in (IReadOnlyList<ToDoList>)(context.Data["Lists"]))
                    {
                        if (list.Id.ToString() == (context.Data["Callback"]).ToString().Split('|')[1])
                        {
                            selectedList = list;
                            context.Data["SelectedList"] = selectedList;
                            break;
                        }
                    }
                    await botClient.SendMessage(message.Chat, $"Подтверждаете удаление списка \"{selectedList.Name}\"", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("✅Да", "yes"), new InlineKeyboardButton("❌Нет", "no")), cancellationToken: ct);
                    context.CurrentStep = "Delete";
                    return ScenarioResult.Transition;
                case "Delete":
                    if (context.Data["Callback"] == "no") 
                    {
                        await botClient.SendMessage(message.Chat, "Удаление отменено.", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    Guid userId = (await _userService.GetUserByTelegramUserIdAsync(message.From.Id, ct)).UserId;
                    Guid listId = ((ToDoList)(context.Data["SelectedList"])).Id;
                    await _toDoListService.Delete(listId, ct);
                    await DirectoryIndexes.RemoveTaskListIndex(listId.ToString());
                    foreach (ToDoItem toDoItem in await _toDoService.GetByUserIdAndList(userId, listId, ct))
                    {
                        await _toDoService.DeleteAsync(toDoItem.Id, ct);
                        await DirectoryIndexes.RemoveTaskIndex(toDoItem.Id.ToString());
                    }
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}
