using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Keyboard;
using Homeworks_otus.TelegramBot.Core.Services;

using Telegram.Bot;
using Telegram.Bot.Types;

using static Homeworks_otus.TelegramBot.Scenarios.ScenarioResultClass;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioTypeClass;

namespace Homeworks_otus.TelegramBot.Scenarios
{
    public class AddListScenario : IScenario
    {
        private readonly IUserService _userService;
        private readonly IToDoListService _toDoListService;
        public AddListScenario(IUserService userService, IToDoListService toDoListService) 
        {
            _userService = userService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Message message, CancellationToken ct)
        {
            switch (context.CurrentStep)
            {
                case null:
                    ToDoUser user = await _userService.GetUserByTelegramUserIdAsync(message.From.Id, ct);
                    context.Data.Add("User", user);
                    await botClient.SendMessage(message.Chat, "Введите название списка", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                    context.CurrentStep = "Name";
                    return ScenarioResult.Transition;
                case "Name":
                    await _toDoListService.Add((ToDoUser)(context.Data["User"]), message.Text, ct);
                    await botClient.SendMessage(message.Chat, $"Лист \"{message.Text}\" успешно добавлен!", replyMarkup: ReplyKeyboard.SetStandardListButton(), cancellationToken: ct);
                    return ScenarioResult.Completed;
            }
            return ScenarioResult.Completed;
        }
    }
}
