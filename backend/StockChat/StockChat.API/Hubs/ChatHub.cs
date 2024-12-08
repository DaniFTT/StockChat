using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StockChat.Bot.MessageQueue;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using StockChat.Domain.Enums;
using System.Security.Claims;
using StockChat.Infrastructure.MessageQueue;

namespace StockChat.API.Hubs;

public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;
    private readonly IMessagePublisher _publisher;

    public ChatHub(IChatService chatService, IAuthService authService, IMessagePublisher publisher)
    {
        _chatService = chatService;
        _authService = authService;
        _publisher = publisher;
    }

    [Authorize]
    public async Task CreateChat(string chatName)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);
        var userEmail = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "Unrecognized user";

        var createChatResult = await _chatService.CreateChatAsync(chatName, userEmail);
        if (!createChatResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Error, createChatResult.Errors.First());
            return;
        }

        var chatId = createChatResult.Value.Id;
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

        var userResult = await _authService.GetCurrentUser();

        var message = $"{userResult.Value.FullName} created the chat '{chatName}'.";
        var addMessageResult = await _chatService.AddMessageAsync(chatId, userId, message, UserType.Admin);
        if (!addMessageResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Error, addMessageResult.Errors.First());
            return;
        }

        await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Success, "Chat created successfully.");
        await Clients.All.SendAsync(HubMessageType.NewChat.ToString(), createChatResult.Value);

        await Clients.Group(chatId.ToString())
            .SendAsync(HubMessageType.NewMessage.ToString(),
                       UserType.Admin,
                       userId,
                       message,
                       addMessageResult.Value.CreatedAt);
    }


    [Authorize]
    public async Task JoinChat(Guid newChatId, Guid? currentChatId = null)
    {
        if(currentChatId is not null)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentChatId.ToString()!);

        await Groups.AddToGroupAsync(Context.ConnectionId, newChatId.ToString());
    }

    [Authorize]
    public async Task SendMessage(Guid chatId, string messageText)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);
        var userResult = await _authService.GetCurrentUser();

        if (messageText.StartsWith("/stock=", StringComparison.OrdinalIgnoreCase))
        {
            await ProcessStockComand(chatId, messageText, userId);
            return;
        }

        var addMessageResult = await _chatService.AddMessageAsync(chatId, userId, messageText);
        if (!addMessageResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Error, addMessageResult.Errors.First());
            return;
        }

        await Clients.Group(chatId.ToString())
             .SendAsync(HubMessageType.NewMessage.ToString(),
                        UserType.AppUser,
                        userResult.Value.FullName,
                        addMessageResult.Value.Text,
                        addMessageResult.Value.CreatedAt);
    }

    private async Task ProcessStockComand(Guid chatId, string messageText, Guid userId)
    {
        var stockCode = messageText.Split('=')[1]?.Trim();
        if (string.IsNullOrEmpty(stockCode))
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Error, "Invalid stock command format. Use /stock=STOCK_CODE.");
            return;
        }

        await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Info, $"Your stock command for '{stockCode}' has been received. Please wait for the quote.");

        var stockMessage = new StockChatQueueMessage
        {
            UserId = userId.ToString(),
            ChatId = chatId.ToString(),
            StockCode = stockCode
        };

        try
        {
            await _publisher.PublishAsync(stockMessage);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), NotificationType.Error, "Failed to process stock command.");
            Console.WriteLine($"Error publishing to RabbitMQ: {ex.Message}");
        }
    }

    [AllowAnonymous]
    public async Task SendStockBotMessage(Guid chatId, Guid userId, string messageText)
    {
        var addMessageResult = await _chatService.AddMessageAsync(chatId, userId, messageText, UserType.StockBot);
        if (!addMessageResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), addMessageResult.Errors.First());
            return;
        }

        await Clients.Group(chatId.ToString())
             .SendAsync(HubMessageType.NewMessage.ToString(),
                        UserType.StockBot,
                        UserType.StockBot.ToString(),
                        addMessageResult.Value.Text,
                        addMessageResult.Value.CreatedAt);
    }

    [Authorize]
    public async Task GetLastMessages(Guid chatId)
    {
        var lastMessages = await _chatService.GetLastMessagesAsync(chatId, 50);

        await Clients.Caller.SendAsync(HubMessageType.GetLastMessages.ToString(), lastMessages.Value);
    }
}

