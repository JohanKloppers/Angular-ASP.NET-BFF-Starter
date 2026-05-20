namespace Starter.Bff.Configuration;

public class ApiSettings
{
    public const string SectionName = "Api";
    public required string BaseUrl { get; init; }
    public required string InternalKey { get; init; }
}
