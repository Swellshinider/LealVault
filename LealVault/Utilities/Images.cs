using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace LealVault.Utilities;

internal static class Images
{
    private static readonly Dictionary<string, Bitmap> CachedImage = [];
    private static readonly Uri ImageResourcePath = new("avares://LealVault/Assets/Images/");

    public static Bitmap GetImage(this ImageKind imageKind)
    {
        var path = imageKind switch
        {
            ImageKind.Wallpaper => "wallpaper.png",
            ImageKind.White_HorizontalLine => "horizontal_line.png",
            ImageKind.White_OpennedEye => "openned_eye.png",
            _ => throw new ArgumentOutOfRangeException(nameof(imageKind), imageKind, null)
        };

        if (CachedImage.TryGetValue(path, out var cache))
            return cache;

        var bitmap = new Bitmap(AssetLoader.Open(new Uri(ImageResourcePath, path)));
        _ = CachedImage.TryAdd(path, bitmap);
        
        return bitmap;
    }
}