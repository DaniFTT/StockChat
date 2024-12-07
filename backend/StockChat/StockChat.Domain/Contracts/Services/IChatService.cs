using Ardalis.Result;
using StockChat.Domain.Entities;
using StockChat.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Domain.Contracts.Services;

public interface IChatService
{
    Task<Result<Chat>> CreateChatAsync(string chatName, string createdBy);
    Task<Result<IEnumerable<Chat>>> GetAllChatsAsync();
    Task<Result<ChatMessage>> AddMessageAsync(Guid chatId, Guid userId, string messageText, UserType userType = UserType.AppUser);
    Task<Result<IEnumerable<ChatMessage>>> GetLastMessagesAsync(Guid chatId, int numberOfMessages);
}

