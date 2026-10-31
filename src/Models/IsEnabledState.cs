namespace BitWallpaper.Models;

public record IsEnabledState
{
    public bool IsEnabled { get; set; }
    public PairCodes PairCode { get; set; }
}
