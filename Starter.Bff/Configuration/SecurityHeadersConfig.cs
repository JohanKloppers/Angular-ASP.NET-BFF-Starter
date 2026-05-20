namespace Starter.Bff.Configuration;

public class SecurityHeadersConfig
{
    public const string SectionName = "SecurityHeaders";
    public required Dictionary<string, string> Headers { get; init; }
}
