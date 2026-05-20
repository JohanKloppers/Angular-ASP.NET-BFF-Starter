namespace Starter.Bff.Models;

public class UserInfo
{
    public required IReadOnlyList<ClaimValue> Claims { get; init; }
}

public class ClaimValue
{
    public required string Type { get; init; }
    public required string Value { get; init; }
}
