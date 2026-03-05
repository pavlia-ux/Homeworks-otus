using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.Services;

using Telegram.Bot;
using Telegram.Bot.Types;

using static Homeworks_otus.TelegramBot.Core.Services.ScenarioContext;

namespace Homeworks_otus.TelegramBot.Core.DataAccess
{
    public interface IScenario
    {
        public enum ScenarioResult
        {
            Transition, Completed
        }

        bool CanHandle(ScenarioType scenario);
        Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Message message, CancellationToken ct);
    }
}
