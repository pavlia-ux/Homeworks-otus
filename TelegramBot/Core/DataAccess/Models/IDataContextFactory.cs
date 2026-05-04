using LinqToDB.Data;

namespace Homeworks_otus.TelegramBot.Core.DataAccess
{
    public interface IDataContextFactory<TDataContext> where TDataContext : DataConnection
    {
        TDataContext CreateDataContext();
    }
}
