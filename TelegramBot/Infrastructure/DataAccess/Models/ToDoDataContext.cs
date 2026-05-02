using Homeworks_otus.TelegramBot.Core.DataAccess.Models;

using LinqToDB;
using LinqToDB.Data;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    public class ToDoDataContext : DataConnection
    {
        public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) 
        {

        }

        public ITable<ToDoUserModel> ToDoUsers => this.GetTable<ToDoUserModel>();
        public ITable<ToDoListModel> ToDoLists => this.GetTable<ToDoListModel>();
        public ITable<ToDoItemModel> ToDoItems => this.GetTable<ToDoItemModel>();
    }
}
