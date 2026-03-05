using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Services;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        private readonly Dictionary<long, ScenarioContext> _context = new();
        public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            if (_context.ContainsKey(userId))
                return _context[userId];
            return null;
        }

        public async Task ResetContext(long userId, CancellationToken ct)
        {
            if (_context.ContainsKey(userId))
                _context.Remove(userId);
        }

        public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            if (_context.ContainsKey(userId))
                _context[userId] = context;
            else
                _context.Add(userId, context);
        }
    }
}
