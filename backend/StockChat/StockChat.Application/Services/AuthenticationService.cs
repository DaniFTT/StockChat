using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using StockChat.Domain.Contracts.Repositories;
using StockChat.Domain.Contracts.Services;
using StockChat.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockChat.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthenticationService(
        IUserRepository userRepository, 
        UserManager<User> userManager, 
        SignInManager<User> signInManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<Result<bool>> LoginAsync(string userEmail, string password)
    {
        if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(password))
            return Result.Error("Email and password are required");

        var user = await _userRepository.GetByEmailAsync(userEmail);
        if (user is null)
            return Result.Error("User not found");

        var result = await _signInManager.PasswordSignInAsync(userEmail, password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
            return Result.Error("Sorry, your login failed!");

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

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
