using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;

namespace StockChat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FullName);

    private readonly IAuthenticationService _authService;

    public AuthenticationController(IAuthenticationService authService)
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

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return NoContent();
    }
}
