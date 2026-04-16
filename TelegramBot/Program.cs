using System;
using System.Collections;
using System.Collections.Generic;

using Homeworks_otus.Core.Services;
using Homeworks_otus.TelegramBot.Core.DataAccess;
using Homeworks_otus.TelegramBot.Core.Services;
using Homeworks_otus.TelegramBot.Infrastructure.DataAccess;
using Homeworks_otus.TelegramBot.Scenarios;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

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
                const string directoryName = "UserDirectory";

                DirectoryIndexes.Initialize(directoryName);

                var fileUserRepository = new FileUserRepository(directoryName);
                var fileToDoRepository = new FileToDoRepository(directoryName); 
                var fileToDoListRepository = new FileToDoListRepository(directoryName);
                var userService = new UserService(fileUserRepository);
                var toDoService = new ToDoService(fileToDoRepository);
                var toDoListService = new ToDoListService(fileToDoListRepository);
                var scenarios = new List<IScenario>()
                {
                    new AddTaskScenario(userService, toDoListService, toDoService),
                    new AddListScenario(userService, toDoListService),
                    new DeleteListScenario(userService, toDoListService, toDoService),
                    new DeleteTaskScenario(toDoService)
                };
                using var cts = new CancellationTokenSource();
                var botClient = new TelegramBotClient(_botKey);
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
                    DropPendingUpdates = true
                };
                var handler = new UpdateHandler(userService, toDoService, new ToDoReportService(toDoService), toDoListService, scenarios, new InMemoryScenarioContextRepository());
                                               
                botClient.StartReceiving(handler, receiverOptions, cts.Token);
                
                var me = await botClient.GetMe();
                await botClient.SetMyCommands(setMyCommands());
                Console.WriteLine($"Бот @{me.Username} запущен. Нажмите клавишу F для выхода.");

                Console.WriteLine("Введите максимально допустимую длину задачи:");
                handler.SetMaxLengthLimit(Console.ReadLine());
                Console.WriteLine("Введите максимально допустимое количество задач:");
                handler.SetMaxQuantityLimit(Console.ReadLine());

                while (Console.ReadKey().Key != ConsoleKey.F)
                {
                    me = await botClient.GetMe();
                    Console.WriteLine($"Информация о боте: @{me.Username}, ID: {me.Id}, Имя: {me.FirstName}");
                }

                //Console.WriteLine("Завершение работы...");
                //cts.Cancel();

                //await Task.Delay(-1); // Устанавливаем бесконечную задержку
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
            new BotCommand("/show", "отображает список всех добавленных задач со статусом Active."),
            new BotCommand("/report", "выводит завершенные/активные задачи на текущий момент."),
            new BotCommand("/find", "отображает список задач пользователя, которые начинаются на введенный префикс."),
            new BotCommand("/cancel", "останавливает сценарии."),
            };
            
            return commands;
        }
    }
}


