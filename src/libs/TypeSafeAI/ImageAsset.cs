namespace TypeSafeAI;

/// <summary>A PNG or JPEG upload. Prepare dimensions and orientation before sending.</summary>
public sealed class ImageAsset
{
    private readonly byte[] _bytes;

    private ImageAsset(byte[] bytes, string format)
    {
        if (bytes.Length == 0 || bytes.Length > 16 * 1024 * 1024)
            throw new ArgumentException("An image must contain between 1 byte and 16 MiB.", nameof(bytes));
        Format = format.ToLowerInvariant() switch
        {
            "png" => "png",
            "jpg" or "jpeg" => "jpg",
            _ => throw new ArgumentException("Only PNG and JPEG images are supported.", nameof(format)),
        };
        _bytes = bytes;
    }

    /// <summary>A unique reference to use as a string anywhere in the request's content.</summary>
    public string Reference { get; } = Guid.NewGuid().ToString();
    public string Format { get; }
    internal byte[] Bytes => _bytes;

    /// <summary>Copies encoded image bytes so callers can safely reuse or modify their buffer.</summary>
    public static ImageAsset FromBytes(ReadOnlySpan<byte> bytes, string format) => new(bytes.ToArray(), format);

    /// <summary>Reads a PNG or JPEG file. The SDK does not resize or rotate it.</summary>
    public static async Task<ImageAsset> FromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        var format = Path.GetExtension(path).TrimStart('.');
        if (new FileInfo(path).Length > 16 * 1024 * 1024)
            throw new ArgumentException("An image cannot exceed 16 MiB.", nameof(path));
        return new ImageAsset(await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false), format);
    }
}
