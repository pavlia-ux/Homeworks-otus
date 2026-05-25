using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;
using Homeworks_otus.TelegramBot.Core.Services;
using Telegram.Bot;

namespace Homeworks_otus.TelegramBot.BackgroundTasks
{
    public class NotificationBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _botClient;
        public NotificationBackgroundTask(TimeSpan delay, INotificationService notificationService, ITelegramBotClient botClient) : base(delay, nameof(NotificationBackgroundTask))
        {
            _notificationService = notificationService;
            _botClient = botClient;
        }
        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);
            foreach (Notification notif in notifications)
            {
                await _botClient.SendMessage(notif.User.TelegramUserId, notif.Text, cancellationToken: ct);
                await _notificationService.MarkNotified(notif.Id, ct);
            }
        }
    }
}
