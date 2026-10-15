using System.IO.Compression;
using System.Net;

namespace TypeSafeAI;

/// <summary>HTTP request body compression supported without additional runtime dependencies.</summary>
public enum RequestCompression
{
    None,
    Gzip,
}

/// <summary>Streams one gzip layer while retaining the JSON or multipart content type.</summary>
internal sealed class GzipRequestContent : HttpContent
{
    private readonly HttpContent _inner;

    internal GzipRequestContent(HttpContent inner)
    {
        _inner = inner;
        foreach (var header in inner.Headers)
        {
            if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
                Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
        Headers.ContentEncoding.Add("gzip");
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        SerializeToStreamAsync(stream, context, CancellationToken.None);

    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        var gzip = new GZipStream(stream, CompressionLevel.Fastest, leaveOpen: true);
        await using (gzip.ConfigureAwait(false))
        {
            await _inner.CopyToAsync(gzip, cancellationToken).ConfigureAwait(false);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        length = 0;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _inner.Dispose();
        base.Dispose(disposing);
    }
}
