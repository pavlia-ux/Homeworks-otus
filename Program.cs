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
using Homeworks_otus;
using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using static System.Net.Mime.MediaTypeNames;
using static Homeworks_otus.ToDoItem;

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

    #region homework4
    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int taskCountLimit)
        {
            Console.WriteLine($"Превышено максимальное количество задач равное {taskCountLimit}");
        }
    }

    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit)
        {
            Console.WriteLine($"Длина задачи {taskLength} превышает максимально допустимое значение {taskLengthLimit}");
        }
    }

    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task)
        {
            Console.WriteLine($"Задача {task} уже существует");
        }
    }
    #endregion

    #region homework5
    public class ToDoUser
    {
        private Guid _userId;
        public long telegramUserId;
        public string telegramUserName;
        private DateTime _registeredAt;
        public ToDoUser(long telegramUserId, string telegramUserName)
        {
            this.telegramUserId = telegramUserId;
            this.telegramUserName = telegramUserName;
            _userId = Guid.NewGuid();
            _registeredAt = DateTime.UtcNow;
        }
        public Guid UserId
        {
            get { return _userId; }
        }
        public long TelegramUserId
        {
            get { return telegramUserId; }
        }
        public string TelegramUserName
        {
            get { return telegramUserName; }
        }
        public DateTime RegisteredAt
        {
            get { return _registeredAt; }
        }
    }

    public class ToDoItem
    {
        public enum ToDoItemState
        {
            Active, Completed
        }

        public Guid Id { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public ToDoItem(ToDoUser user, string name)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.UtcNow;
            State = ToDoItemState.Active;
        }
    }
    #endregion

    #region homework6
    public class UpdateHandler : IUpdateHandler
    {
        public static ToDoUser user = null;

        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;
        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                //botClient.SendMessage(update.Message.Chat, "Вам доступны команды /start, /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /exit. Выбирайте :)");

                if (update.Message.Text.Equals("/start"))
                {
                    botClient.SendMessage(update.Message.Chat, $"{Start(botClient, update)}");
                }
                else if (update.Message.Text.Equals("/help"))
                {
                    botClient.SendMessage(update.Message.Chat, $"{Help()}");
                }
                else if (update.Message.Text.Equals("/info"))
                {
                    botClient.SendMessage(update.Message.Chat, $"{Info()}");
                }
                else if (update.Message.Text.Contains("/addtask") && _userService.GetUser(update.Message.From.Id) != null)
                {
                    _toDoService.Add(_userService.GetUser(update.Message.From.Id), update.Message.Text.Replace("/addtask", "").Trim());
                    botClient.SendMessage(update.Message.Chat, $"Задача добавлена");
                }
                else if (update.Message.Text.Equals("/showtasks") && _userService.GetUser(update.Message.From.Id) != null)
                {
                    ShowTasks(botClient, update);
                }
                else if (update.Message.Text.Contains("/removetask") && _userService.GetUser(update.Message.From.Id) != null)
                {
                    //_toDoService.Delete(_userService.GetUser(update.Message.From.Id), update.Message.Text.Replace("/removetask", "").Trim());
                    //botClient.SendMessage(update.Message.Chat, $"Задача удалена");
                }
                else if (update.Message.Text.Contains("/completetask") && _userService.GetUser(update.Message.From.Id) != null)
                {
                    //int.TryParse(Console.ReadLine(), out int taskToDelete);
                    //_toDoService.MarkCompleted(Guid.Parse(update.Message.Text.Replace("/completetask", "").Trim(), out Guid guidCompleted));
                    botClient.SendMessage(update.Message.Chat, $"Задача отмечена как выполненная");
                }
                else if (update.Message.Text.Equals("/showalltasks") && _userService.GetUser(update.Message.From.Id) != null)
                {
                    ShowAllTasks(botClient, update);
                }
                else if (update.Message.Text.Equals("/exit"))
                {
                    //Как прервать работу?
                }
                else
                {
                    botClient.SendMessage(update.Message.Chat, "простите, но пока что я Вас не поняла :(");
                }
            }
            catch (TaskCountLimitException taskCountEx)
            {
                botClient.SendMessage(update.Message.Chat, taskCountEx.Message);
            }
            catch (TaskLengthLimitException taskLengthEx)
            {
                botClient.SendMessage(update.Message.Chat, taskLengthEx.Message);
            }
            catch (DuplicateTaskException duplicateTaskEx)
            {
                botClient.SendMessage(update.Message.Chat, duplicateTaskEx.Message);
            }
            catch (ArgumentException argEx)
            {
                botClient.SendMessage(update.Message.Chat, argEx.Message);
            }
            catch (Exception ex)
            {
                botClient.SendMessage(update.Message.Chat, $"Произошла непредвиденная ошибка: {ex.GetType().FullName} | {ex.Message} | {ex.StackTrace} | {ex.InnerException}");
            }
        }

        #region homework2
        public string Start(ITelegramBotClient botClient, Update update)
        {
            ToDoUser? User = _userService.GetUser(update.Message.From.Id);
            if (User == null)
            {
                User = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
                return $"{User.TelegramUserName}, теперь Вы зарегистрированы! Вам доступны команды /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /exit";
            }
            else 
            {
                return $"{User.TelegramUserName}, добрый день! Вам доступны команды / help, / info, / addtask, / showtasks, / removetask, / completetask, / showalltasks, / exit";
            }
        }
        public string Help()
        {
            string strHelp = "в этой программе следующий список доступных команд: /start, /help, " +
                        "/info, /exit.\r\n" +
                        "/start - программа просит Вас ввести своё имя, также сохраняет Ваш Id и дату регистрации.\r\n" +
                        "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                        "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                        "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                        "/showtasks - отображает список всех добавленных задач со статусом Active.\r\n" +
                        "/removetask - позволяет удалять задачи по номеру в общем списке.\r\n" +
                        "/completetask - позволяет ставить отметку о выполнении задачи по ее Id.\r\n" +
                        "/showalltasks - отображает список всех добавленных задач.\r\n" +
                        "/exit - программа заканчивает свою работу.";
            return strHelp;
        }
        public string Info()
        {
            string strInfo = "программа v4 создана 20.01.2026";
            return strInfo;
        }
        #endregion

        #region homework3
        public void ShowTasks(ITelegramBotClient botClient, Update update)
        {
            Guid guidUserId = _userService.GetUser(update.Message.From.Id).UserId;
            var activeToDoItems = _toDoService.GetActiveByUserId(guidUserId);

            if (activeToDoItems.Count > 0)
            {
                int a = 1;
                for (int i = 0; i < activeToDoItems.Count; i++)
                {
                    botClient.SendMessage(update.Message.Chat, $"{a}. {activeToDoItems[i].Name} - {activeToDoItems[i].CreatedAt} - {activeToDoItems[i].Id}");
                    a++;
                }
            }

            else
            {
                botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст");
            }
        }
        #endregion

        #region homework5
        public void ShowAllTasks(ITelegramBotClient botClient, Update update)
        {
            Guid guidUserId = _userService.GetUser(update.Message.From.Id).UserId;
            var toDoItems = _toDoService.GetAllByUserId(guidUserId);

            if (toDoItems.Count > 0)
            {
                for (int i = 0; i < toDoItems.Count; i++)
                {
                    if (toDoItems[i].State == ToDoItemState.Completed)
                    {
                        botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems[i].State} - {toDoItems[i].StateChangedAt}| {toDoItems[i].Name} - {toDoItems[i].CreatedAt} - {toDoItems[i].Id}");
                    }
                    else
                    {
                        botClient.SendMessage(update.Message.Chat, $"{i + 1}. |{toDoItems[i].State}| {toDoItems[i].Name} - {toDoItems[i].CreatedAt} - {toDoItems[i].Id}");
                    }
                }
            }

            else
            {
                botClient.SendMessage(update.Message.Chat, "кажется, список задач пуст");
            }
        }
        #endregion

    }
    public class UserService : IUserService
    {
        private readonly List<ToDoUser> _users = new List<ToDoUser>();
        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            ToDoUser user = new ToDoUser(telegramUserId, telegramUserName);
            _users.Add(user);
            return user;
        }
        public ToDoUser? GetUser(long telegramUserId)
        {
            ToDoUser? User = _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
            if (User != null)
                return User;
            else
                return null;
        }
    }

    public class ToDoService : IToDoService
    {
        public const int min = 1;
        public const int max = 100;
        private readonly List<ToDoItem> _toDoItems = new List<ToDoItem>();
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            IReadOnlyList<ToDoItem> allToDoItems = _toDoItems.FindAll(n => n.User.UserId == userId);
            return allToDoItems;
        }
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            IReadOnlyList<ToDoItem> activeToDoItems = _toDoItems.FindAll(n => n.State == ToDoItemState.Active && n.User.UserId == userId);
            return activeToDoItems;
        }
        public ToDoItem Add(ToDoUser user, string name)
        {
            int parsedTaskCountLimit = ParseAndValidateInt(name, min, max);
            int parsedTaskLengthLimit = ParseAndValidateInt(name, min, max);
            ToDoItem toDoItem = new ToDoItem(user, name);

            if (_toDoItems.Count >= parsedTaskCountLimit)
            {
                throw new TaskCountLimitException(parsedTaskCountLimit);
            }

            if (name.Length > parsedTaskLengthLimit)
            {
                throw new TaskLengthLimitException(name.Length, parsedTaskLengthLimit);
            }

            if (ValidateString(name) == true)
            {
                throw new Exception("Вы ввели пробелы или пустую строку");
            }

            _toDoItems.Add(toDoItem);
            return toDoItem;
        }
        public void MarkCompleted(Guid id)
        {
            foreach (ToDoItem toDo in _toDoItems)
            {
                if (toDo.Id == id)
                {
                    toDo.State = ToDoItemState.Completed;
                    toDo.StateChangedAt = DateTime.UtcNow;
                    break;
                }
            }
        }
        public void Delete(Guid id)
        {
            if (_toDoItems.Count > 0)
            {
                //string strYourTasks = "Ваш список задач:";
                //Console.WriteLine(strYourTasks);

                //ShowTasks();

                //Console.WriteLine("Введите номер задачи для удаления");
                //bool successfulParse = int.TryParse(Console.ReadLine(), out int taskToDelete);
                //if (successfulParse == true)
                //{
                //    while (taskToDelete > toDoItems.Count)
                //    {
                //        Console.WriteLine("Введен некорректный номер задачи. Пожалуйста, попробуйте еще раз");
                //        successfulParse = int.TryParse(Console.ReadLine(), out taskToDelete);
                //        if (successfulParse == false)
                //        {
                //            Console.WriteLine("Не удалось преобразовать строку в число, попробуйте еще раз");
                //            RemoveTask();
                //        }
                //    }

                //    for (int i = 0; i < toDoItems.Count; i++)
                //    {
                //        if (i == taskToDelete - 1)
                //        {
                //            toDoItems.RemoveAt(i); ;
                //            Console.WriteLine("Задача удалена из списка");
                //        }
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("Не удалось преобразовать строку в число, попробуйте еще раз");
                //    RemoveTask();
                //}
            }

            else
            {
                string strEmptyTasks = "кажется, список задач пуст";
                Console.WriteLine(strEmptyTasks);
            }
        }

        #region homework4
        public static int ParseAndValidateInt(string? str, int min, int max)
        {
            int parsedStr;

            if (int.TryParse(str, out parsedStr) == true)
            {
                parsedStr = int.Parse(str);
            }
            else
            {
                throw new ArgumentException("Ошибка! Не удалось преобразовать строку в число");
            }

            if (parsedStr < min || parsedStr > max)
            {
                throw new ArgumentException("Ошибка! Число должно находиться в диапазоне от 1 до 100 (включительно)");
            }
            else
            {
                return parsedStr;
            }
        }
        public static bool ValidateString(string? str)
        {
            bool strIsNullOrWhiteSpace = string.IsNullOrWhiteSpace(str);
            return strIsNullOrWhiteSpace;
        }
        #endregion
    }
    #endregion
}


