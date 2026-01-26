using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homeworks_otus.Core.Exceptions
{
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit)
        {
            Console.WriteLine($"Длина задачи {taskLength} превышает максимально допустимое значение {taskLengthLimit}");
        }
    }
}
