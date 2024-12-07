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

    public async Task<Result<UserDto>> LoginAsync(string userEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(password))
            return Result.Error("Email and password are required");

        var userResult = await _userRepository.GetByEmailAsync(userEmail);
        if (!userResult.IsSuccess)
            return Result.Error("User not found");

        var result = await _signInManager.PasswordSignInAsync(userEmail, password, isPersistent: false, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Result.Error("Sorry, your login failed!");

        var user = userResult.Value!;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.FullName), 
        };

        var identity = new ClaimsIdentity(claims, "ApplicationCookie");
        var principal = new ClaimsPrincipal(identity);

        await _httpContextAccessor.HttpContext!.SignInAsync(principal);

        return Result.Success(new UserDto(user.Email!, user.FullName));
    }

    public async Task<Result<UserDto>> RegisterAsync(User user, string password)
    {
        if (user is null || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(password))
            return Result.Error("Email and password are required");

        var userAlreadyExists = await _userRepository.GetByEmailAsync(user.Email!);
        if (userAlreadyExists.IsSuccess)
            return Result.Error("User email already exists");

        user.UserName = user.Email;

        var result = await _userRepository.CreateUserAsync(user, password);

        return Result.Success(new UserDto(user.Email!, user.FullName));
    }

    public async Task<Result<UserDto>> GetCurrentUser()
    {
        var userId = GetClaimValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Result.Error("User ID not found in claims. Please log in again.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Error("User not found. The account might have been removed.");

        return Result.Success(new UserDto(user.Email!, user.FullName));
    }

    private string? GetClaimValue(string claimType)
    {
        var claims = _httpContextAccessor.HttpContext?.User?.Claims;
        if (claims == null || !claims.Any())
            return null;

        return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
