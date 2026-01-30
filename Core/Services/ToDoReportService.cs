using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Entities;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.Core.Services
{
    public class ToDoReportService : IToDoReportService
    {
        private readonly IToDoService _toDoService;
        public ToDoReportService(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var result = (total: 0, completed: 0, active: 0, generatedAt: DateTime.UtcNow);

            result.total = _toDoService.GetAllByUserId(userId).Count();
            result.completed = _toDoService.GetAllByUserId(userId).Where(x => x.State == ToDoItemState.Completed).Count();
            result.active = _toDoService.GetActiveByUserId(userId).Where(x => x.State == ToDoItemState.Active).Count();

            return result;
        }
    }
}
