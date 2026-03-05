using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess;

namespace Homeworks_otus.TelegramBot.Core.Services
{
    public class ScenarioContext 
    {
        public enum ScenarioType
        {
            None, AddTask
        }

        public ScenarioType CurrentScenario { get; set; }
        public string? CurrentStep { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public ScenarioContext(ScenarioType scenario)
        {
            CurrentScenario = scenario;
            CurrentStep = null;
            Data = new Dictionary<string, object>();
        }
    }
}
