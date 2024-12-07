using Ardalis.Result;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Dtos;
using StockChat.Domain.Entities;
using System.Security.Claims;


namespace StockChat.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IUserRepository userRepository, 
        UserManager<User> userManager, 
        SignInManager<User> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _signInManager = signInManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<bool>> LoginAsync(string userEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(password))
            return Result.Error("Email and password are required");

        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (!user.IsSuccess)
            return Result.Error("User not found");

        var result = await _signInManager.PasswordSignInAsync(userEmail, password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Result.Error("Sorry, your login failed!");

        var cookies = _httpContextAccessor.HttpContext.Response.Headers["Set-Cookie"];
        foreach (var cookie in cookies)
        {
            Console.WriteLine($"Set-Cookie: {cookie}");
        }

        return result.Succeeded;
    }

    public async Task<Result<bool>> RegisterAsync(User user, string password)
    {
        if (user is null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(password))
            return Result.Error("Email and password are required");

        var userAlreadyExists = await _userRepository.GetByEmailAsync(user.Email!);
        if (userAlreadyExists.IsSuccess)
            return Result.Error("User email already exists");

        user.UserName = user.Email;

        var result = await _userRepository.CreateUserAsync(user, password);

        return result;
    }

    public async Task<Result<UserDto>> GetCurrentUser()
    {
        var userClaims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (userClaims == null || !userClaims.Any())
            return Result.Error("No claims found. User might not be authenticated.");

        var userIdClaim = userClaims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Result.Error("User ID not found in claims. User might not be authenticated.");

        var user = await _userManager.FindByIdAsync(userIdClaim);
        if (user is null)
            return Result.Error("User not found.");

        var userDto = new UserDto(user.Email!, user.FullName);

        return Result.Success(userDto);
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
