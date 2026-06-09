namespace ChatApp_BE.Infrastructure.Configuration;

public sealed class ChatCorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = ["http://localhost:3000"];
}
