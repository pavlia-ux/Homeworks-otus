using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Services;

namespace Homeworks_otus.TelegramBot.BackgroundTasks
{
    public class DeadlineBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;
        public DeadlineBackgroundTask(TimeSpan delay, INotificationService notificationService, IUserRepository userRepository, IToDoRepository toDoRepository) : base(delay, nameof(DeadlineBackgroundTask))
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _toDoRepository = toDoRepository;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            var users = await _userRepository.GetAllUsers(ct);
            foreach (var user in users)
            {
                var tasks = await _toDoRepository.GetActiveWithDeadline(user.UserId, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, ct);
                foreach (var task in tasks)
                {
                    await _notificationService.ScheduleNotification(user.UserId, $"Deadline_{task.Id}", $"Ой! Вы пропустили дедлайн по задаче \"{task.Name}\"", DateTime.UtcNow, ct);
                }
            }
        }
    }
}
