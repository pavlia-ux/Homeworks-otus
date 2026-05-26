using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Entities;
using Homeworks_otus.TelegramBot.Infrastructure.DataAccess;
using Homeworks_otus.TelegramBot.Infrastructure.DataAccess.Models;

using LinqToDB;
using LinqToDB.Async;

namespace Homeworks_otus.TelegramBot.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<ToDoDataContext> _factory;
        public NotificationService(IDataContextFactory<ToDoDataContext> dataContextFactory)
        {
            _factory = dataContextFactory;
        }
        public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var models = await AsyncExtensions.ToListAsync(dbContext.Notifications
                .Where(n => !n.IsNotified && n.ScheduledAt <= scheduledBefore)
                .LoadWith(n => n.User));
            return models.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var model = await dbContext.Notifications.FirstOrDefaultAsync(n => n.ForeignId == notificationId);
            if (model == null)
                throw new Exception("Такого уведомления не существует");
            model.IsNotified = true;
            model.NotifiedAt = DateTime.UtcNow;
            await dbContext.UpdateAsync(model, token: ct);
        }

        public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
        {
            using var dbContext = _factory.CreateDataContext();
            var user = await dbContext.ToDoUsers.FirstOrDefaultAsync(u => u.ForeignId == userId);
            if (await dbContext.Notifications.AnyAsync(n => n.UserId == user.Id && n.Type == text))
                return false;
            Notification notification = new Notification()
            {
                Id = Guid.NewGuid(),
                User = ModelMapper.MapFromModel(user),
                Type = type,
                Text = text,
                ScheduledAt = scheduledAt
            };
            NotificationModel model = ModelMapper.MapToModel(notification);
            await dbContext.InsertAsync(model);
            return true;
        }
    }
}
