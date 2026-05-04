using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess.Models;
using Homeworks_otus.TelegramBot.Core.Entities;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            return new ToDoUser
            {
                UserId = model.ForeignId,
                DatabaseId = model.Id,
                TelegramUserId = model.TelegramUserId,
                TelegramUserName = model.TelegramUserName,
                RegisteredAt = model.RegisteredAt
            };
        }

        public static ToDoUserModel MapToModel(ToDoUser entity)
        {
            return new ToDoUserModel
            {
                ForeignId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            return new ToDoItem
            {
                UserId = model.ForeignId,
                DatabaseId = model.Id,
                Id = model.ForeignId,
                UserDatabaseId = model.UserId,
                Name = model.ItemName,
                CreatedAt = model.ItemCreatedAt,
                State = (ToDoItemState)model.ToDoItemState,
                DeadLine = model.DeadLine,
                StateChangedAt = model.StateChangedAt,
                ToDoListDatabaseId = model.ToDoListId,
                ToDoList = new ToDoList
                {
                    Id = model.ToDoList.ForeignId,
                    Name = model.ToDoList.ListName,
                    CreatedAt = model.ToDoList.ListCreatedAt,
                    DatabaseId = model.ToDoList.Id,
                    UserDatabaseId = model.ToDoList.UserId,
                },
                User = new ToDoUser
                {
                    UserId = model.User.ForeignId,
                    DatabaseId = model.User.Id,
                    RegisteredAt = model.User.RegisteredAt,
                    TelegramUserId = model.User.TelegramUserId,
                    TelegramUserName = model.User.TelegramUserName
                }
            };
        }

        public static ToDoItemModel MapToModel(ToDoItem entity)
        {
            return new ToDoItemModel
            {
                ForeignId = entity.Id,
                ItemName = entity.Name,
                ItemCreatedAt = entity.CreatedAt,
                ToDoItemState = (int)entity.State,
                DeadLine = entity.DeadLine,
                StateChangedAt = entity.StateChangedAt ?? DateTime.UtcNow,
                UserId = entity.UserDatabaseId ?? 0,
                ToDoListId = entity.ToDoListDatabaseId
            };
        }

        public static ToDoList MapFromModel(ToDoListModel model)
        {
            return new ToDoList
            {
                Id = model.ForeignId,
                DatabaseId = model.Id,
                Name = model.ListName,
                UserDatabaseId = model.UserId,
                CreatedAt = model.ListCreatedAt,
                User = new ToDoUser
                {
                    DatabaseId = model.User.Id,
                    UserId = model.User.ForeignId,
                    RegisteredAt = model.User.RegisteredAt,
                    TelegramUserId = model.User.TelegramUserId,
                    TelegramUserName = model.User.TelegramUserName
                }
            };
        }

        public static ToDoListModel MapToModel(ToDoList entity)
        {
            return new ToDoListModel
            {
                ForeignId = entity.Id,
                ListName = entity.Name,
                UserId = entity.UserDatabaseId ?? 0,
                ListCreatedAt = entity.CreatedAt
            };
        }
    }
}
