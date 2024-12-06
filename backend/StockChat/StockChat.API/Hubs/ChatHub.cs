using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Enums;

namespace StockChat.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task CreateChat(string chatName)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);
        var userName = Context.User?.Identity?.Name ?? "Unreconized user";

        var createChatResult = await _chatService.CreateChatAsync(chatName);

        if (!createChatResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), createChatResult.Errors.First());
            return;
        }

        var chatId = createChatResult.Value.Id;
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

        await Clients.Group(chatId.ToString())
            .SendAsync(HubMessageType.NewMessage.ToString(),
                       UserType.Admin,
                       $"{userName} created the chat '{chatName}'.");

        await Clients.All.SendAsync(HubMessageType.NewChat.ToString(), createChatResult.Value);
    }

    public async Task JoinChat(Guid chatId)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);

        await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());

        await Clients.Group(chatId.ToString())
             .SendAsync(HubMessageType.Notification.ToString(),
                        UserType.Admin,
                        $"{userId} joined the chat.");
    }

    public async Task SendMessage(Guid chatId, string messageText)
    {
        var userId = Guid.Parse(Context.UserIdentifier!);

        var addMessageResult = await _chatService.AddMessageAsync(chatId, userId, messageText);
        if(!addMessageResult.IsSuccess)
        {
            await Clients.Caller.SendAsync(HubMessageType.Notification.ToString(), addMessageResult.Errors.First());
            return;
        }

        await Clients.Group(chatId.ToString())
             .SendAsync(HubMessageType.NewMessage.ToString(),
                        UserType.AppUser,
                        addMessageResult.Value.UserId,
                        addMessageResult.Value.Text,
                        addMessageResult.Value.CreatedAt);
    }

    public async Task GetLastMessages(Guid chatId)
    {
        var lastMessages = await _chatService.GetLastMessagesAsync(chatId, 50);

        await Clients.Caller.SendAsync(HubMessageType.GetLastMessages.ToString(), lastMessages);
    }
}

