using Starter.Bff.Services;

namespace Starter.Bff.Tests.Services;

public class UserServiceTests
{
    private readonly UserService _sut = new();

    [Fact]
    public void GetUserInfo_ReturnsAllowedClaims()
    {
        var principal = MakePrincipal(
            ("sub", "user-123"),
            ("email", "test@example.com"),
            ("name", "Test User"),
            ("firstName", "Test"),
            ("lastName", "User"));

        var result = _sut.GetUserInfo(principal);

        Assert.Contains(result.Claims, c => c.Type == "sub" && c.Value == "user-123");
        Assert.Contains(result.Claims, c => c.Type == "email" && c.Value == "test@example.com");
        Assert.Contains(result.Claims, c => c.Type == "name" && c.Value == "Test User");
        Assert.Contains(result.Claims, c => c.Type == "firstName" && c.Value == "Test");
        Assert.Contains(result.Claims, c => c.Type == "lastName" && c.Value == "User");
    }

    [Fact]
    public void GetUserInfo_FiltersOutDisallowedClaims()
    {
        var principal = MakePrincipal(
            ("sub", "user-123"),
            ("someInternalClaim", "secret-value"),
            ("aud", "should-be-filtered"));

        var result = _sut.GetUserInfo(principal);

        Assert.DoesNotContain(result.Claims, c => c.Type == "someInternalClaim");
        Assert.DoesNotContain(result.Claims, c => c.Type == "aud");
    }

    [Fact]
    public void GetUserInfo_WithNoClaims_ReturnsEmptyList()
    {
        var principal = MakePrincipal();

        var result = _sut.GetUserInfo(principal);

        Assert.Empty(result.Claims);
    }

    [Fact]
    public void GetUserInfo_WithDuplicateClaims_ReturnsBoth()
    {
        var principal = MakePrincipal(
            ("email", "first@example.com"),
            ("email", "second@example.com"));

        var result = _sut.GetUserInfo(principal);

        Assert.Equal(2, result.Claims.Count(c => c.Type == "email"));
    }

    private static System.Security.Claims.ClaimsPrincipal MakePrincipal(params (string type, string value)[] claims)
    {
        var identity = new ClaimsIdentity(
            claims.Select(c => new Claim(c.type, c.value)));
        return new ClaimsPrincipal(identity);
    }
}
