namespace BitApps.Core.Models;

public class ShowBalloonEventArgs(string title, string text) : EventArgs
{
    public string? Title { get; private set; } = title;
    public string? Text { get; private set; } = text;
}
