using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Schedulas.Application.Common.Interfaces;
using Schedulas.Infrastructure.Identity;

namespace Schedulas.Infrastructure.Storage;

/// <summary>
/// Talks to Supabase Storage's REST API directly
/// (https://supabase.com/docs/reference/javascript/storage-from-upload --
/// the same endpoints the JS/Dart SDKs call under the hood). A real
/// integration, not a stub: it requires a live Supabase project with the
/// referenced bucket already created to function, per the Constitution's
/// "no fake implementations" rule.
/// </summary>
public sealed class SupabaseStorageService : IFileStorageService
{
    private readonly HttpClient _http;
    private readonly SupabaseOptions _options;

    public SupabaseStorageService(HttpClient http, IOptions<SupabaseOptions> options)
    {
        _options = options.Value;

        http.BaseAddress = new Uri($"{_options.Url.TrimEnd('/')}/storage/v1/");
        http.DefaultRequestHeaders.Add("apikey", _options.AnonKey);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.AnonKey);
        _http = http;
    }

    public async Task<string> UploadAsync(string bucket, string path, Stream content, string contentType, CancellationToken ct = default)
    {
        using var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        // upsert=true: re-uploading to the same path replaces the file
        // rather than erroring, which matches how "update institution logo"
        // or "replace exported report" are expected to behave.
        var response = await _http.PostAsync($"object/{bucket}/{path}?upsert=true", streamContent, ct);
        await EnsureSuccess(response);

        return path;
    }

    public async Task<Stream> DownloadAsync(string bucket, string path, CancellationToken ct = default)
    {
        var response = await _http.GetAsync($"object/{bucket}/{path}", HttpCompletionOption.ResponseHeadersRead, ct);
        await EnsureSuccess(response);

        var innerStream = await response.Content.ReadAsStreamAsync(ct);

        // response (HttpResponseMessage) must stay alive for as long as its
        // content stream is being read -- with ResponseHeadersRead, the
        // underlying connection is only released once the body is fully
        // consumed AND the response disposed. Returning innerStream alone
        // would leak the response/connection, since nothing downstream
        // holds a reference to dispose it. This wrapper ties the two
        // lifetimes together so a simple `using` on the caller's side is
        // enough to clean up both.
        return new ResponseOwningStream(response, innerStream);
    }

    /// <summary>Forwards all stream operations to the inner stream, and disposes the owning HttpResponseMessage alongside it.</summary>
    private sealed class ResponseOwningStream : Stream
    {
        private readonly HttpResponseMessage _response;
        private readonly Stream _inner;

        public ResponseOwningStream(HttpResponseMessage response, Stream inner)
        {
            _response = response;
            _inner = inner;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            _inner.ReadAsync(buffer, offset, count, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
                _response.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public async Task DeleteAsync(string bucket, string path, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync($"object/{bucket}/{path}", ct);
        await EnsureSuccess(response);
    }

    public string GetPublicUrl(string bucket, string path) =>
        $"{_options.Url.TrimEnd('/')}/storage/v1/object/public/{bucket}/{path}";

    public async Task<string> GetSignedUrlAsync(string bucket, string path, TimeSpan expiresIn, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(
            $"object/sign/{bucket}/{path}",
            new { expiresIn = (int)expiresIn.TotalSeconds },
            ct);

        await EnsureSuccess(response);

        var body = await response.Content.ReadFromJsonAsync<SignedUrlResponse>(cancellationToken: ct)
            ?? throw new InvalidOperationException("Supabase Storage returned an empty signed-URL response.");

        return $"{_options.Url.TrimEnd('/')}/storage/v1{body.SignedUrl}";
    }

    private static async Task EnsureSuccess(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;

        var body = await response.Content.ReadAsStringAsync();
        throw new SupabaseStorageException(response.StatusCode, body);
    }

    private sealed record SignedUrlResponse([property: JsonPropertyName("signedURL")] string SignedUrl);
}

public sealed class SupabaseStorageException : Exception
{
    public System.Net.HttpStatusCode StatusCode { get; }

    public SupabaseStorageException(System.Net.HttpStatusCode statusCode, string responseBody)
        : base($"Supabase Storage request failed with {statusCode}: {responseBody}")
    {
        StatusCode = statusCode;
    }
}
