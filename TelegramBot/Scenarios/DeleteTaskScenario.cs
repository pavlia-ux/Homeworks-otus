using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Keyboard;
using Homeworks_otus.TelegramBot.Core.Services;
using Homeworks_otus.TelegramBot.Dto;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

using static Homeworks_otus.TelegramBot.Scenarios.ScenarioResultClass;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioTypeClass;

namespace Homeworks_otus.TelegramBot.Scenarios
{
    internal class DeleteTaskScenario : IScenario
    {
        private readonly IToDoService _toDoService;
        public DeleteTaskScenario(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }
        public bool CanHandle(ScenarioType scenario) => scenario == ScenarioType.DeleteTask;

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    Guid taskId = (Guid)ToDoItemCallbackDto.FromString(context.Data["Callback"].ToString()).ToDoItemId;
                    context.Data.Add("taskId", taskId.ToString());
                    string taskName = (await _toDoService.Get(taskId, ct)).Name;
                    await botClient.SendMessage(message.Chat, $"Подтверждаете удаление задачи \"{taskName}\"", replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton("✅Да", "yes"), new InlineKeyboardButton("❌Нет", "no")), cancellationToken: ct);
                    context.CurrentStep = "Delete";
                    return ScenarioResult.Transition;
                case "Delete":
                    if (context.Data["Callback"].ToString() == "no")
                    {
                        await botClient.SendMessage(message.Chat, "Удаление отменено.", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                        return ScenarioResult.Completed;
                    }
                    await _toDoService.DeleteAsync(Guid.Parse((context.Data["taskId"].ToString())), ct);
                    await botClient.SendMessage(message.Chat, "Задача удалена.", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                    break;
            }
            return ScenarioResult.Completed;
        }
    }
}
