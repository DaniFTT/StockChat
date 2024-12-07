using Microsoft.AspNetCore.Mvc;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;

namespace StockChat.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FullName);

    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);
        
        return NoContent();
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var user = new User
        {
            Email = request.Email,
            FullName = request.FullName,
        };

        var result = await _authService.RegisterAsync(user, request.Password);

        if (!result.IsSuccess)
            return BadRequest(result.Errors);
        
        return NoContent();
    }

    [HttpGet("current-user")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var result = await _authService.GetCurrentUser();
        if (!result.IsSuccess)
            return Unauthorized(result.Errors);

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return NoContent();
    }
}
