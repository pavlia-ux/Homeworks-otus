using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Homeworks_otus.Core.DataAccess;
using Homeworks_otus.Core.Services;
using Homeworks_otus.Infrastructure.DataAccess;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using static Telegram.Bot.TelegramBotClient;

namespace Homeworks_otus
{
    internal class Program
    {
        // Get token from environment variable
        private static string _botKey = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN", EnvironmentVariableTarget.User);

        private static async Task Main(string[] args)
        {
            try
            {
                if (string.IsNullOrEmpty(_botKey))
                {
                    Console.WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                    return;
                }

                var inMemoryUserRepository = new InMemoryUserRepository();
                var inMemoryToDoRepository = new InMemoryToDoRepository();
                var toDoService = new ToDoService(inMemoryToDoRepository);
                using var cts = new CancellationTokenSource();
                var botClient = new TelegramBotClient(_botKey);
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message],
                    DropPendingUpdates = true
                };
                var handler = new UpdateHandler(new UserService(inMemoryUserRepository), toDoService, new ToDoReportService(toDoService));
                                               
                botClient.StartReceiving(handler, receiverOptions, cts.Token);
                User user = await botClient.GetMe();
                await botClient.SetMyCommands(setMyCommands());
                Console.WriteLine($"{user.FirstName} запущен!");

                if (Console.ReadKey().Key == ConsoleKey.F)
                {
                    Console.WriteLine("Асинхронные операции отменены.");
                    cts.Cancel();
                }
                else
                {
                    Console.WriteLine($@"Информация о боте: {user.Username}");
                }

                await Task.Delay(-1); // Устанавливаем бесконечную задержку
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType().FullName} | {ex.Message} | {ex.StackTrace} | {ex.InnerException}");
            }
        }
        private static List<BotCommand> setMyCommands()
        {
            List<BotCommand> commands = new List<BotCommand>() {new BotCommand("/start", "программа просит Вас ввести своё имя."),
            new BotCommand("/help", "отображает краткую справочную информацию."),
            new BotCommand("/info", "предоставляет информацию о версии программы и дате её создания."),
            new BotCommand("/addtask", "позволяет добавлять задачи в список (по одной)."),
            new BotCommand("/showtasks", "отображает список всех добавленных задач со статусом Active."),
            new BotCommand("/removetask", "позволяет удалять задачи по номеру в общем списке."),
            new BotCommand("/completetask", "позволяет ставить отметку о выполнении задачи по ее Id."),
            new BotCommand("/showalltasks", "отображает список всех добавленных задач."),
            new BotCommand("/report", "выводит завершенные/активные задачи на текущий момент."),
            new BotCommand("/find", "отображает список задач пользователя, которые начинаются на введенный префикс."),
            };
            
            return commands;
        }
    }
}


