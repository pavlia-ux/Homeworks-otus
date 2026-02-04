using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Services;
using Homeworks_otus.Infrastructure.DataAccess;

using Otus.ToDoList.ConsoleBot;

namespace Homeworks_otus
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var inMemoryUserRepository = new InMemoryUserRepository();
                var inMemoryToDoRepository = new InMemoryToDoRepository();
                var toDoService = new ToDoService(inMemoryToDoRepository);
                var handler = new UpdateHandler(new UserService(inMemoryUserRepository), toDoService, new ToDoReportService(toDoService));
                var botClient = new ConsoleBotClient();
                Console.Write("Введите максимально допустимую длину задачи: ");
                handler.SetMaxLengthLimit(Console.ReadLine());
                Console.Write("Введите максимально допустимое количество задач: ");
                handler.SetMaxQuantityLimit(Console.ReadLine());
                botClient.StartReceiving(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType().FullName} | {ex.Message} | {ex.StackTrace} | {ex.InnerException}");
            }
        }
    }
}


