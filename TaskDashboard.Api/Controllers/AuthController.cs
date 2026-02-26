using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskDashboard.Api.DTOs;
using TaskDashboard.Api.Models;
using TaskDashboard.Api.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly ActivityLogService _activityLog;

    public AuthController(UserManager<AppUser> userManager, TokenService tokenService, ActivityLogService activityLog)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _activityLog = activityLog;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "User");
        var token = _tokenService.CreateToken(user, "User");

        await _activityLog.LogAsync(user.Id, "Register", "User", details: $"User {dto.FullName} registered");

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = "User",
            PreferredLanguage = user.PreferredLanguage,
            PreferredTheme = user.PreferredTheme,
            HasCompletedOnboarding = user.HasCompletedOnboarding,
            AvatarUrl = user.AvatarUrl
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password" });

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";
        var token = _tokenService.CreateToken(user, role);

        await _activityLog.LogAsync(user.Id, "Login", "User");

        return Ok(new AuthResponseDto
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = role,
            PreferredLanguage = user.PreferredLanguage,
            PreferredTheme = user.PreferredTheme,
            HasCompletedOnboarding = user.HasCompletedOnboarding,
            AvatarUrl = user.AvatarUrl
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new AuthResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = roles.FirstOrDefault() ?? "User",
            PreferredLanguage = user.PreferredLanguage,
            PreferredTheme = user.PreferredTheme,
            HasCompletedOnboarding = user.HasCompletedOnboarding,
            AvatarUrl = user.AvatarUrl
        });
    }
}
