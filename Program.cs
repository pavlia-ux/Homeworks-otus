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
using Homeworks_otus.Core.Services;
using Otus.ToDoList.ConsoleBot;

namespace Homeworks_otus
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var handler = new UpdateHandler(new UserService(), new ToDoService());
                var botClient = new ConsoleBotClient();
                botClient.StartReceiving(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType().FullName} | {ex.Message} | {ex.StackTrace} | {ex.InnerException}");
            }
        }
    }
}


