using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Homeworks_otus
{
    internal class Program
    {
        public const int min = 1;
        public const int max = 100;
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
            Console.WriteLine("Вам доступны команды /start, /help, /info, /addtask, /showtasks, /removetask, /exit. Выбирайте :)");
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
                else if (line.Contains("/exit"))
                {
                    IsRun = false;
                }
                else
                {
                    switch (Name)
                    {
                        case "":
                            Console.WriteLine("Простите, но пока что я Вас не поняла :(");
                            break;
                        default:
                            Console.WriteLine($"Простите, {Name}, но пока что я Вас не поняла :(");
                            break;
                    }
                }
            }
            while (IsRun == true);
        }

        #region Homework2
        public static string Name = "";
        public static void Start()
        {
            Console.WriteLine("Пожалуйста, введите свое имя");
            Name = Console.ReadLine();
            Console.WriteLine($"{Name}, теперь Вам доступна команда /echo. Введите команду /echo и любой текст");
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
                        "/start - программа просит Вас ввести своё имя.\r\n" +
                        "/echo - при вводе этой команды с аргументом (например, /echo Hello), программа возвращает введенный текст \r\n" +
                        "(в данном примере \"Hello\").\r\n" +
                        "/help - отображает краткую справочную информацию о том, как пользоваться программой. \r\n" +
                        "/info - предоставляет информацию о версии программы и дате её создания.\r\n" +
                        "/addtask - позволяет добавлять задачи в список (по одной).\r\n" +
                        "/showtasks - отображает список всех добавленных задач.\r\n" +
                        "/removetask - позволяет удалять задачи по номеру в общем списке.\r\n" +
                        "/exit - программа заканчивает свою работу.";
            Console.WriteLine(ValidateString(Name) ? strHelp : Name + ", " + strHelp);
        }

        public static void Info()
        {
            string strInfo = "программа v3 создана 23.12.2025";
            Console.WriteLine(ValidateString(Name) ? strInfo : Name + ", " + strInfo);
        }
        #endregion

        #region Homework3
        public static List<string> tasks = new List<string>();

        public static void AddTask(int parsedTaskCountLimit, int parsedTaskLengthLimit)
        {
            string strAddTask = "пожалуйста, введите описание задачи";
            Console.WriteLine(ValidateString(Name) ? strAddTask : Name + ", " + strAddTask);
            string task = Console.ReadLine();

            if (tasks.Count >= parsedTaskCountLimit)
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
                if (!tasks.Contains(task))
                {
                    tasks.Add(task);
                    Console.WriteLine($"Задача \"{task}\" добавлена");
                }
                else
                {
                    throw new DuplicateTaskException(task);
                }
            }
        }

        public static void ShowTasks()
        {
            if (tasks.Count > 0)
            {
                for (int i = 0; i < tasks.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {tasks[i]}");
                }
            }

            else
            {
                string strEmptyTasks = "кажется, список задач пуст";
                Console.WriteLine(ValidateString(Name) ? strEmptyTasks : Name + ", " + strEmptyTasks);
            }
        }

        public static void RemoveTask()
        {
            if (tasks.Count > 0)
            {
                string strYourTasks = "Ваш список задач:";
                Console.WriteLine(ValidateString(Name) ? strYourTasks : Name + ", " + strYourTasks);

                ShowTasks();

                Console.WriteLine("Введите номер задачи для удаления");
                bool successfulParse = int.TryParse(Console.ReadLine(), out int taskToDelete);
                if (successfulParse == true)
                {
                    while (taskToDelete > tasks.Count)
                    {
                        Console.WriteLine("Введен некорректный номер задачи. Пожалуйста, попробуйте еще раз");
                        successfulParse = int.TryParse(Console.ReadLine(), out taskToDelete);
                        if (successfulParse == false)
                        {
                            Console.WriteLine("Не удалось преобразовать строку в число, попробуйте еще раз");
                            RemoveTask();
                        }
                    }

                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if (i == taskToDelete - 1)
                        {
                            tasks.RemoveAt(i);
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
                Console.WriteLine(ValidateString(Name) ? strEmptyTasks : Name + ", " + strEmptyTasks);
            }
        }
        #endregion

        #region Homework4
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

    #region Homework4
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
}

