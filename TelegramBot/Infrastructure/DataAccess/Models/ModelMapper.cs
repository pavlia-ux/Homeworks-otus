using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Homeworks_otus.Core.Entities;
using Homeworks_otus.TelegramBot.Core.DataAccess.Models;
using Homeworks_otus.TelegramBot.Core.Entities;

using Telegram.Bot.Types;

using static Homeworks_otus.Core.Entities.ToDoItem;

namespace Homeworks_otus.TelegramBot.Infrastructure.DataAccess
{
    internal static class ModelMapper
    {
        public static ToDoUser MapFromModel(ToDoUserModel model)
        {
            return new ToDoUser
            {
                UserId = model.ExternalId,
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
                ExternalId = entity.UserId,
                TelegramUserId = entity.TelegramUserId,
                TelegramUserName = entity.TelegramUserName,
                RegisteredAt = entity.RegisteredAt
            };
        }

        public static ToDoItem MapFromModel(ToDoItemModel model)
        {
            return new ToDoItem
            {
                UserId = model.ExternalId,
                DatabaseId = model.Id,
                Id = model.ExternalId,
                UserDatabaseId = model.UserId,
                Name = model.ItemName,
                CreatedAt = model.CreatedAt,
                State = (ToDoItemState)model.ItemState,
                DeadLine = model.DeadLine,
                StateChangedAt = model.StateChangedAt,
                ToDoListDatabaseId = model.ToDoListId,
                ToDoList = new ToDoList
                {
                    Id = model.List.ExternalId,
                    Name = model.List.ListName,
                    CreatedAt = model.List.CreatedAt,
                    DatabaseId = model.List.Id,
                    UserDatabaseId = model.List.UserId,
                },
                User = new ToDoUser
                {
                    UserId = model.User.ExternalId,
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
                ExternalId = entity.Id,
                ItemName = entity.Name,
                CreatedAt = entity.CreatedAt,
                ItemState = (int)entity.State,
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
                Id = model.ExternalId,
                DatabaseId = model.Id,
                Name = model.ListName,
                UserDatabaseId = model.UserId,
                CreatedAt = model.CreatedAt,
                User = new ToDoUser
                {
                    DatabaseId = model.User.Id,
                    UserId = model.User.ExternalId,
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
                ExternalId = entity.Id,
                ListName = entity.Name,
                UserId = entity.UserDatabaseId ?? 0,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
