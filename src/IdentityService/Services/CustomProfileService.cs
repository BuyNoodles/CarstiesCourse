using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public class CustomProfileService(UserManager<ApplicationUser> userManager)
    : IProfileService
{

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await userManager.GetUserAsync(context.Subject);
        var existingClaims = await userManager.GetClaimsAsync(user!);

        List<Claim> claims = [ new Claim("username", user!.UserName!) ];

        context.IssuedClaims.AddRange(claims);
        
        context.IssuedClaims.Add(
            existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)!);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}