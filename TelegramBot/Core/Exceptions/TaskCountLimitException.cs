namespace Homeworks_otus.Core.Exceptions
{
    public class TaskCountLimitException : Exception
    {
        public TaskCountLimitException(int taskCountLimit)
        {
            Console.WriteLine($"Превышено максимальное количество задач равное {taskCountLimit}");
        }
    }
}
