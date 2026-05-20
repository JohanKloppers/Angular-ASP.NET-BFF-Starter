using System.Security.Claims;
using Starter.Bff.Models;

namespace Starter.Bff.Services;

public interface IUserService
{
    UserInfo GetUserInfo(ClaimsPrincipal user);
}

public class UserService : IUserService
{
    private static readonly HashSet<string> AllowedClaimTypes =
    [
        ClaimTypes.NameIdentifier,
        ClaimTypes.Email,
        ClaimTypes.Role,
        "sub",
        "email",
        "name",
        "firstName",
        "lastName",
    ];

    public UserInfo GetUserInfo(ClaimsPrincipal user)
    {
        var claims = user.Claims
            .Where(c => AllowedClaimTypes.Contains(c.Type))
            .Select(c => new ClaimValue { Type = c.Type, Value = c.Value })
            .ToList();

        return new UserInfo { Claims = claims };
    }
}
