using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Otus.ToDoList.ConsoleBot.Types;

namespace Homeworks_otus.Core.Exceptions
{
    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task)
        {
            Console.WriteLine($"Задача {task} уже существует");
        }
    }
}
