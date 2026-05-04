using Homeworks_otus.TelegramBot.Core.Services;

using Telegram.Bot;
using Telegram.Bot.Types;

using static Homeworks_otus.TelegramBot.Scenarios.ScenarioResultClass;
using static Homeworks_otus.TelegramBot.Scenarios.ScenarioTypeClass;

namespace Homeworks_otus.TelegramBot.Core.DataAccess
{
    public interface IScenario
    {
        bool CanHandle(ScenarioType scenario);
        Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct);
    }
}
