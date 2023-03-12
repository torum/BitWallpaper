namespace BitWallpaper.Models;

public class ShowBalloonEventArgs : EventArgs
{
    public string? Title { get; set; }
    public string? Text { get; set; }
}
