using Ardalis.Result;
using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Application.Services;

public class ChatService : IChatService
{
    private readonly IChatRepository _chatRepository;
    private readonly IChatMessageRepository _chatMessageRepository;

    public ChatService(IChatRepository chatRepository,
                       IChatMessageRepository chatMessageRepository)
    {
        _chatRepository = chatRepository;
        _chatMessageRepository = chatMessageRepository;
    }

    public async Task<Result<Chat>> CreateChatAsync(string chatName)
    {
        var chat = new Chat
        {
            ChatName = chatName, 
        };

        await _chatRepository.AddAsync(chat);

        return Result.Success(chat);
    }

    public async Task<Result<ChatMessage>> AddMessageAsync(Guid chatId, Guid userId, string messageText)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId);
        if (chat is null)
            return Result.Error("Chat not found.");

        var message = new ChatMessage
        {
            ChatId = chat.Id,
            UserId = userId,
            Text = messageText
        };

        await _chatMessageRepository.AddAsync(message);

        return message;
    }


    public async Task<Result<IEnumerable<ChatMessage>>> GetLastMessagesAsync(Guid chatId, int count)
    {
        var messages = await _chatMessageRepository.GetLastMessagesAsync(chatId, count);

        return Result.Success(messages);
    }
}

