using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskDashboard.Api.DTOs;
using TaskDashboard.Api.Models;
using TaskDashboard.Api.Services;

namespace TaskDashboard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ActivityLogService _activityLog;

    public ProfileController(UserManager<AppUser> userManager, ActivityLogService activityLog)
    {
        _userManager = userManager;
        _activityLog = activityLog;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPut]
    public async Task<ActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        if (dto.FullName != null) user.FullName = dto.FullName;
        if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;
        if (dto.PreferredLanguage != null) user.PreferredLanguage = dto.PreferredLanguage;
        if (dto.PreferredTheme != null) user.PreferredTheme = dto.PreferredTheme;

        await _userManager.UpdateAsync(user);
        await _activityLog.LogAsync(UserId, "Updated", "Profile");

        return Ok(new { message = "Profile updated" });
    }

    [HttpPut("password")]
    public async Task<ActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _activityLog.LogAsync(UserId, "Changed Password", "Profile");
        return Ok(new { message = "Password changed" });
    }

    [HttpPut("onboarding")]
    public async Task<ActionResult> CompleteOnboarding()
    {
        var user = await _userManager.FindByIdAsync(UserId);
        if (user == null) return NotFound();

        user.HasCompletedOnboarding = true;
        await _userManager.UpdateAsync(user);

        return Ok(new { message = "Onboarding completed" });
    }
}
