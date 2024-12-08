using Ardalis.Result;
using Bogus;
using Microsoft.AspNetCore.SignalR;
using Moq;
using StockChat.API.Hubs;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using StockChat.Domain.Enums;
using StockChat.Infrastructure.MessageQueue;
using System.Security.Claims;

namespace StockChat.UnitTest;

public class ChatHubTests
{
    private readonly Mock<IChatService> _chatServiceMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly Mock<IHubCallerClients> _clientsMock;
    private readonly Mock<ISingleClientProxy> _callerMock;
    private readonly Mock<HubCallerContext> _contextMock;
    private readonly Mock<IGroupManager> _groupsMock;

    private readonly ChatHub _chatHub;
    private readonly Faker _faker;

    private Guid _userId;
    private string _connectionId;
    private string _userFullName;
    private ClaimsPrincipal _userClaims;

    public ChatHubTests()
    {
        _chatServiceMock = new Mock<IChatService>();
        _authServiceMock = new Mock<IAuthService>();
        _publisherMock = new Mock<IMessagePublisher>();
        _clientsMock = new Mock<IHubCallerClients>();
        _callerMock = new Mock<ISingleClientProxy>();
        _contextMock = new Mock<HubCallerContext>();
        _groupsMock = new Mock<IGroupManager>();

        _chatHub = new ChatHub(_chatServiceMock.Object, _authServiceMock.Object, _publisherMock.Object)
        {
            Clients = _clientsMock.Object,
            Context = _contextMock.Object,
            Groups = _groupsMock.Object
        };

        _faker = new Faker();

        _connectionId = Guid.NewGuid().ToString();
        _userId = Guid.NewGuid();
        _userFullName = _faker.Person.FullName;

        _userClaims = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
            new Claim(ClaimTypes.Name, _userFullName)
        }));

        _contextMock.Setup(m => m.User).Returns(_userClaims);
        _contextMock.Setup(m => m.UserIdentifier).Returns(_userId.ToString());
        _contextMock.Setup(m => m.ConnectionId).Returns(_connectionId);
        _clientsMock.Setup(m => m.Caller).Returns(_callerMock.Object);
        _clientsMock.Setup(m => m.All).Returns(Mock.Of<ISingleClientProxy>(proxy =>
            proxy.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default) == Task.CompletedTask));

        _groupsMock.Setup(m => m.AddToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
           .Returns(Task.CompletedTask);

        _groupsMock.Setup(m => m.RemoveFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), default))
                   .Returns(Task.CompletedTask);

        _authServiceMock
            .Setup(m => m.GetCurrentUser())
            .ReturnsAsync(Result.Success(new Domain.Dtos.UserDto(_faker.Internet.Email(), _userFullName)));

        _chatHub.Groups = _groupsMock.Object;
        _chatHub.Clients = _clientsMock.Object;
        _chatHub.Context = _contextMock.Object;
    }

    private void SetupCommonMocks(Guid chatId)
    {
        var groupMock = new Mock<IClientProxy>();
        _clientsMock.Setup(m => m.Group(chatId.ToString())).Returns(groupMock.Object);
    }


    [Fact]
    public async Task SendMessage_ValidMessage_Should_AddMessageToChatAndNotifyGroup()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var messageText = _faker.Lorem.Sentence();

        var chatMessage = new ChatMessage
        {
            ChatId = chatId,
            UserId = _userId,
            Text = messageText,
            CreatedAt = DateTimeOffset.UtcNow
        };

        SetupCommonMocks(chatId);

        _chatServiceMock
            .Setup(m => m.AddMessageAsync(chatId, _userId, messageText, UserType.AppUser))
            .ReturnsAsync(chatMessage);

        // Act
        await _chatHub.SendMessage(chatId, messageText);

        // Assert
        _chatServiceMock.Verify(m => m.AddMessageAsync(chatId, _userId, messageText, UserType.AppUser), Times.Once);

        _clientsMock.Verify(m => m.Group(chatId.ToString())
            .SendCoreAsync("NewMessage", It.Is<object[]>(args =>
                args[0].Equals(UserType.AppUser) &&
                args[1].Equals(_userFullName) &&
                args[2].Equals(messageText) &&
                args[3].Equals(chatMessage.CreatedAt)), default), Times.Once);
    }

    [Fact]
    public async Task SendMessage_InvalidStockCommand_Should_SendErrorNotification()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var invalidCommand = "/stock=";

        SetupCommonMocks(chatId);

        // Act
        await _chatHub.SendMessage(chatId, invalidCommand);

        // Assert
        _clientsMock.Verify(m => m.Caller.SendCoreAsync("Notification", It.Is<object[]>(args =>
            args[0].Equals(NotificationType.Error) &&
            args[1].Equals("Invalid stock command format. Use /stock=STOCK_CODE.")), default), Times.Once);
    }

    [Fact]
    public async Task JoinChat_ValidChatId_Should_AddConnectionToGroup()
    {
        // Arrange
        var newChatId = Guid.NewGuid();

        SetupCommonMocks(newChatId);

        // Act
        await _chatHub.JoinChat(newChatId);

        // Assert
        _groupsMock.Verify(m => m.AddToGroupAsync(_connectionId, newChatId.ToString(), default), Times.Once);
    }

    [Fact]
    public async Task CreateChat_ValidChat_Should_NotifyClients()
    {
        // Arrange
        var chatId = Guid.NewGuid();
        var chatName = _faker.Commerce.ProductName();

        var chat = new Chat
        {
            Id = chatId,
            ChatName = chatName
        };

        SetupCommonMocks(chatId);

        var chatMessage = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            UserId = _userId,
            Text = $"{_userFullName} created the chat '{chatName}'.",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _chatServiceMock
            .Setup(m => m.AddMessageAsync(chatId, _userId, chatMessage.Text, UserType.Admin))
            .ReturnsAsync(Result.Success(chatMessage)); 

        _chatServiceMock
            .Setup(m => m.CreateChatAsync(chatName, It.IsAny<string>()))
            .ReturnsAsync(Result.Success(chat));

        // Act
        await _chatHub.CreateChat(chatName);

        // Assert
        _chatServiceMock.Verify(m => m.CreateChatAsync(chatName, It.IsAny<string>()), Times.Once);

        _chatServiceMock.Verify(m => m.AddMessageAsync(chatId, _userId, $"{_userFullName} created the chat '{chatName}'.", UserType.Admin), Times.Once);

        _clientsMock.Verify(m => m.All.SendCoreAsync(
            "NewChat",
            It.Is<object[]>(args => ((Chat)args[0]).ChatName == chatName),
            default), Times.Once);

        _clientsMock.Verify(m => m.Group(chatId.ToString()).SendCoreAsync(
            "NewMessage",
            It.Is<object[]>(args =>
                args[0].Equals(UserType.Admin) &&
                args[1].Equals(_userId) &&
                args[2].Equals(chatMessage.Text) &&
                args[3].Equals(chatMessage.CreatedAt)),
            default), Times.Once);

        _clientsMock.Verify(m => m.Caller.SendCoreAsync(
            "Notification",
            It.Is<object[]>(args =>
                args[0].Equals(NotificationType.Success) &&
                args[1].Equals("Chat created successfully.")),
            default), Times.Once);
    }
}
