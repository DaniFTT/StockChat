using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using StockChat.Domain.Enums;
using System.Security.Claims;

namespace StockChat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly IAuthService _authService;

    public ChatHub(IChatService chatService, IAuthService authService)
    {
        _chatService = chatService;
        _authService = authService;
    }

    public async Task CreateChat(string chatName)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);
        var userEmail = Context.User?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "Unrecognized user";

        var createChatResult = await _chatService.CreateChatAsync(chatName, userEmail);
        if (!createChatResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), createChatResult.Errors.First());
            return;
        }

        var chatId = createChatResult.Value.Id;
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

        var userResult = await _authService.GetCurrentUser();

        var message = $"{userResult.Value.FullName} created the chat '{chatName}'.";
        var addMessageResult = await _chatService.AddMessageAsync(chatId, userId, message, UserType.Admin);
        if (!addMessageResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), addMessageResult.Errors.First());
            return;
        }

        await Clients.All.SendAsync(HubMessageType.NewChat.ToString(), createChatResult.Value);

        await Clients.Group(chatId.ToString())
            .SendAsync(HubMessageType.NewMessage.ToString(),
                       UserType.Admin,
                       userId,
                       message,
                       addMessageResult.Value.CreatedAt);
    }

    public async Task JoinChat(Guid chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
    }

    public async Task SendMessage(Guid chatId, string messageText)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);
        var userResult = await _authService.GetCurrentUser();

        var addMessageResult = await _chatService.AddMessageAsync(chatId, userId, messageText);
        if(!addMessageResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), addMessageResult.Errors.First());
            return;
        }

        await Clients.Group(chatId.ToString())
             .SendAsync(HubMessageType.NewMessage.ToString(),
                        UserType.AppUser,
                        userResult.Value.FullName,
                        addMessageResult.Value.Text,
                        addMessageResult.Value.CreatedAt);
    }

    public async Task GetLastMessages(Guid chatId)
    {
        var lastMessages = await _chatService.GetLastMessagesAsync(chatId, 50);

        await Clients.Caller.SendAsync(HubMessageType.GetLastMessages.ToString(), lastMessages.Value);
    }
}

