namespace Homeworks_otus.Core.Exceptions
{
    public class TaskLengthLimitException : Exception
    {
        public TaskLengthLimitException(int taskLength, int taskLengthLimit)
        {
            Console.WriteLine($"Длина задачи {taskLength} превышает максимально допустимое значение {taskLengthLimit}");
        }
    }
}
