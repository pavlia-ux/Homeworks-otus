using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.TelegramBot.BackgroundTasks.Interfaces
{
    public interface IBackgroundTask
    {
        Task Start(CancellationToken ct);
    }
}
