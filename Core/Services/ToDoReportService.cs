using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            
        }
    }
}
