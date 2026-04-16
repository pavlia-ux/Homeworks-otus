using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.Entities;

using LinqToDB;
using LinqToDB.Data;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    public class ToDoDataContext : DataConnection
    {
        public ToDoDataContext(string connectionString) : base(ProviderName.PostgreSQL, connectionString) 
        {

        }

        public ITable<ToDoUser> ToDoUsers => this.GetTable<ToDoUser>();
        public ITable<ToDoList> ToDoLists => this.GetTable<ToDoList>();
        public ITable<ToDoItem> ToDoItems => this.GetTable<ToDoItem>();
    }
}
