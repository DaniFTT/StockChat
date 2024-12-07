using Microsoft.AspNetCore.Mvc;
using StockChat.Domain.Contracts.Services;

namespace StockChat.API.Controllers;

[ApiController]
[Route("api/chats")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpGet]
    public async Task<IActionResult> GetChats()
    {
        var chats = await _chatService.GetAllChatsAsync();
        return Ok(chats.Value);
    }
}

