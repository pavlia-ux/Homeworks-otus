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
using static Homeworks_otus.ToDoItem;

namespace Homeworks_otus
{
    internal class Program
    {
        public const int min = 1;
        public const int max = 100;
        public static ToDoUser _user = null;
        public static List<ToDoItem> _toDoItems = new List<ToDoItem>();
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Добрый день! Введите максимально допустимое количество задач (1-100)");
                int parsedTaskCountLimit = ParseAndValidateInt(Console.ReadLine(), min, max);

                Console.WriteLine("Введите максимально допустимую длину задачи (1-100)");
                int parsedTaskLengthLimit = ParseAndValidateInt(Console.ReadLine(), min, max);

                Commands(parsedTaskCountLimit, parsedTaskLengthLimit);
            }
            catch (TaskCountLimitException taskCountEx)
            {
                Console.WriteLine(taskCountEx.Message);
            }
            catch (TaskLengthLimitException taskLengthEx)
            {
                Console.WriteLine(taskLengthEx.Message);
            }
            catch (DuplicateTaskException duplicateTaskEx)
            {
                Console.WriteLine(duplicateTaskEx.Message);
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла непредвиденная ошибка: {ex.GetType().FullName} | {ex.Message} | {ex.StackTrace} | {ex.InnerException}");
            }
        }
        public static void Commands(int parsedTaskCountLimit, int parsedTaskLengthLimit)
        {
            Console.WriteLine("Вам доступны команды /start, /help, /info, /addtask, /showtasks, /removetask, /completetask, /showalltasks, /exit. Выбирайте :)");
            bool IsRun = true;
            do
            {
                string line = Console.ReadLine();

                if (line.Equals("/start"))
                {
                    Start();
                }
                else if (line.StartsWith("/echo"))
                {
                    Echo(line);
                }
                else if (line.Equals("/help"))
                {
                    Help();
                }
                else if (line.Equals("/info"))
                {
                    Info();
                }
                else if (line.Equals("/addtask"))
                {
                    AddTask(parsedTaskCountLimit, parsedTaskLengthLimit);
                }
                else if (line.Equals("/showtasks"))
                {
                    ShowTasks();
                }
                else if (line.Equals("/removetask"))
                {
                    RemoveTask();
                }
                else if (line.Equals("/completetask"))
                {
                    CompleteTask();
                }
                else if (line.Equals("/showalltasks"))
                {
                    ShowAllTasks();
                }
                else if (line.Contains("/exit"))
                {
                    IsRun = false;
                }
                else
                {
                    string strError = "простите, но пока что я Вас не поняла :(";
                    Console.WriteLine(ValidateString(_user.TelegramUserName) ? strError : _user.TelegramUserName + ", " + strError);
                }
            }
            while (IsRun == true);
        }

        #region homework2
        public static void Start()
        {
            var userName = GetUserName();
            _user = new ToDoUser(userName);

            Console.WriteLine($"Привет, {_user.TelegramUserName}! Чем могу помочь?");
        }
        public static void Echo(string echoConsole)
        {
            if (ValidateString(echoConsole.Replace("/echo", "")) == false)
            {
                string echo = echoConsole.Substring(echoConsole.Split(' ').First().Length);
                Console.WriteLine(echo.Trim(' '));
            }
            else
            {
                Console.WriteLine("Строка пустая или содержит значение NULL");
            }
        }
        public static void Help()
        {
            string strHelp = "в этой программе следующий список доступных команд: /start, /echo (доступна после внесения имени в команде /start)," +
                        "/help, /info, /exit.\r\n" +
                        "/start - программа просит Вас ввести своё имя, также сохраняет Ваш Id и дату регистрации.\r\n" +
                        "/echo - при вводе этой команды с аргументом (например, /echo Hello), программа возвращает введенный текст \r\n" +
                        "(в данном примере \"Hello\").\r\n" +
                        "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                        "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                        "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                        "/showtasks - отображает список всех добавленных задач со статусом Active.\r\n" +
                        "/removetask - позволяет удалять задачи по номеру в общем списке.\r\n" +
                        "/completetask - позволяет ставить отметку о выполнении задачи по ее Id.\r\n" +
                        "/showalltasks - отображает список всех добавленных задач.\r\n" +
                        "/exit - программа заканчивает свою работу.";
            Console.WriteLine(ValidateString(_user.TelegramUserName) ? strHelp : _user.TelegramUserName + ", " + strHelp);
        }
        public static void Info()
        {
            string strInfo = "программа v3 создана 23.12.2025";
            Console.WriteLine(ValidateString(_user.TelegramUserName) ? strInfo : _user.TelegramUserName + ", " + strInfo);
        }
        #endregion

        #region homework3
        public static void AddTask(int parsedTaskCountLimit, int parsedTaskLengthLimit)
        {
            string strAddTask = "пожалуйста, введите описание задачи";
            Console.WriteLine(ValidateString(_user.TelegramUserName) ? strAddTask : _user.TelegramUserName + ", " + strAddTask);
            string task = Console.ReadLine();

            if (_toDoItems.Count >= parsedTaskCountLimit)
            {
                throw new TaskCountLimitException(parsedTaskCountLimit);
            }

            if (task.Length > parsedTaskLengthLimit)
            {
                throw new TaskLengthLimitException(task.Length, parsedTaskLengthLimit);
            }

            if (ValidateString(task) == true)
            {
                Console.WriteLine("Вы ввели пробелы или пустую строку");
            }

            else
            {
                if (!_user.TelegramUserName.Contains(task))
                {
                    ToDoItem _ToDoItem = new ToDoItem(_user, task);
                    _toDoItems.Add(_ToDoItem);
                    Console.WriteLine($"Задача \"{ task}\" добавлена");
                }
                else
                {
                    throw new DuplicateTaskException(task);
                }
            }
        }
        public static void ShowTasks()
        {
            if (_toDoItems.Count > 0)
            {
                int a = 1;
                for (int i = 0; i < _toDoItems.Count; i++)
                {
                    if (_toDoItems[i].State == ToDoItemState.Active)
                    {
                        Console.WriteLine($"{a}. {_toDoItems[i].Name} - {_toDoItems[i].CreatedAt} - {_toDoItems[i].Id}");
                        a++;
                    }
                }
            }

            else
            {
                string strEmptyTasks = "кажется, список задач пуст";
                Console.WriteLine(ValidateString(_user.TelegramUserName) ? strEmptyTasks : _user.TelegramUserName + ", " + strEmptyTasks);
            }
        }
        public static void RemoveTask()
        {
            if (_toDoItems.Count > 0)
            {
                string strYourTasks = "Ваш список задач:";
                Console.WriteLine(ValidateString(_user.TelegramUserName) ? strYourTasks : _user.TelegramUserName + ", " + strYourTasks);

                ShowTasks();

                Console.WriteLine("Введите номер задачи для удаления");
                bool successfulParse = int.TryParse(Console.ReadLine(), out int taskToDelete);
                if (successfulParse == true)
                {
                    while (taskToDelete > _toDoItems.Count)
                    {
                        Console.WriteLine("Введен некорректный номер задачи. Пожалуйста, попробуйте еще раз");
                        successfulParse = int.TryParse(Console.ReadLine(), out taskToDelete);
                        if (successfulParse == false)
                        {
                            Console.WriteLine("Не удалось преобразовать строку в число, попробуйте еще раз");
                            RemoveTask();
                        }
                    }

                    for (int i = 0; i < _toDoItems.Count; i++)
                    {
                        if (i == taskToDelete - 1)
                        {
                            _toDoItems.RemoveAt(i); ;
                            Console.WriteLine("Задача удалена из списка");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Не удалось преобразовать строку в число, попробуйте еще раз");
                    RemoveTask();
                }
            }

            else
            {
                string strEmptyTasks = "кажется, список задач пуст";
                Console.WriteLine(ValidateString(_user.TelegramUserName) ? strEmptyTasks : _user.TelegramUserName + ", " + strEmptyTasks);
            }
        }
        #endregion

        #region homework5
        public static void CompleteTask()
        {
            if (_toDoItems.Count > 0)
            {
                Console.WriteLine("Введите Id задачи для проставления статуса \"Выполнена\"");
                string completeTask = Console.ReadLine();

                if (ValidateString(completeTask) == false)
                {
                    for (int i = 0; i < _toDoItems.Count; i++)
                    {
                        if (_toDoItems[i].Id.ToString() == completeTask)
                        {
                            _toDoItems[i].State = ToDoItemState.Completed;
                            _toDoItems[i].StateChangedAt = DateTime.UtcNow;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Вы ввели пробелы или пустую строку");
                }
                Console.WriteLine("Задача выполнена");
            }

            else
            {
                string strEmptyTasks = "кажется, список задач пуст";
                Console.WriteLine(ValidateString(_user.TelegramUserName) ? strEmptyTasks : _user.TelegramUserName + ", " + strEmptyTasks);
            }
        }
        public static void ShowAllTasks()
        {
            if (_toDoItems.Count > 0)
            {
                for (int i = 0; i < _toDoItems.Count; i++)
                {
                    if (_toDoItems[i].State == ToDoItemState.Completed)
                    {
                        Console.WriteLine($"{i + 1}. |{_toDoItems[i].State} - {_toDoItems[i].StateChangedAt}| {_toDoItems[i].Name} - {_toDoItems[i].CreatedAt} - {_toDoItems[i].Id}");
                    }
                    else
                    {
                        Console.WriteLine($"{i + 1}. |{_toDoItems[i].State}| {_toDoItems[i].Name} - {_toDoItems[i].CreatedAt} - {_toDoItems[i].Id}");
                    }
                }
            }

            else
            {
                string strEmptyTasks = "кажется, список задач пуст";
                Console.WriteLine(ValidateString(_user.TelegramUserName) ? strEmptyTasks : _user.TelegramUserName + ", " + strEmptyTasks);
            }
        }
        #endregion

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

        #region homework5
        static string GetUserName()
        {
            string? name;
            do
            {
                Console.WriteLine("Пожалуйста, введите свое имя");
                name = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(name));
            return name;
        }
        #endregion
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
        public string _telegramUserName;
        private DateTime _registeredAt;
        public ToDoUser(string telegramUserName)
        {
            _telegramUserName = telegramUserName;
            _userId = Guid.NewGuid();
            _registeredAt = DateTime.UtcNow;
        }
        public Guid UserId
        {
            get { return _userId; }
        }
        public string TelegramUserName
        {
            get { return _telegramUserName; }
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
}


