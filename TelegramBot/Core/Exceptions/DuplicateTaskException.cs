namespace Homeworks_otus.Core.Exceptions
{
    public class DuplicateTaskException : Exception
    {
        public DuplicateTaskException(string task)
        {
            Console.WriteLine($"Задача или лист {task} уже существует");
        }
    }
}
