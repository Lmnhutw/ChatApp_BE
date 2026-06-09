namespace ChatApp_BE.ViewModels.Realtime;

public sealed class RealtimeErrorEvent
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
