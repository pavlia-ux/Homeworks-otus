using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Services;

namespace Homeworks_otus.TelegramBot.BackgroundTasks
{
    public class TodayBackgroundTask : BackgroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IToDoRepository _toDoRepository;
        public TodayBackgroundTask(TimeSpan delay, INotificationService notificationService, IUserRepository userRepository, IToDoRepository toDoRepository) : base(delay, nameof(TodayBackgroundTask))
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
                string taskList = string.Empty;
                foreach (var task in tasks)
                {
                    taskList += $"{task.Id}){task.Name}\r\n";
                }
                if (tasks.Count > 0)
                    await _notificationService.ScheduleNotification(user.UserId, $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}", taskList, DateTime.UtcNow, ct);
            }
        }
    }
}
