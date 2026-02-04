using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace WPFLauncherApi.Http;

public class HttpWrapper : IDisposable {
    private readonly string _baseUrl;

    private readonly HttpRequestOptions _defaultOptions;

    public HttpWrapper(string baseUrl = "", Action<HttpRequestOptions>? configureDefaults = null,
        HttpClientHandler? handler = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        Client = new HttpClient(handler ?? new HttpClientHandler {
            AutomaticDecompression = DecompressionMethods.All
        }) {
            Timeout = TimeSpan.FromSeconds(30L)
        };
        _defaultOptions = new HttpRequestOptions();
        configureDefaults?.Invoke(_defaultOptions);
        ApplyDefaultOptions();
    }

    private HttpClient Client { get; }

    public void Dispose()
    {
        Client.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<string> GetStringAsync(string url, Action<HttpRequestOptions>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var obj = await GetAsync(url, configure, cancellationToken);
        obj.EnsureSuccessStatusCode();
        return await obj.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<string> GetStringAsync1(
        string url,
        Action<HttpWrapperBuilder>? block = null,
        CancellationToken cancellationToken = default)
    {
        var obj = await GetAsync1(url, block, cancellationToken);
        obj.EnsureSuccessStatusCode();
        return await obj.Content.ReadAsStringAsync(cancellationToken);
    }

    public async Task<T?> GetAsync<T>(string url, Action<HttpRequestOptions>? configure = null,
        CancellationToken cancellationToken = default)
    {
        return JsonSerializer.Deserialize<T>(await GetStringAsync(url, configure, cancellationToken));
    }

    public async Task<HttpResponseMessage> GetAsync(string url, Action<HttpRequestOptions>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, url, configure);
        return await Client.SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> GetAsync1(string url, Action<HttpWrapperBuilder>? block = null,
        CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, url, string.Empty, block);
        return await Client.SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T data,
        Action<HttpRequestOptions>? configure = null, CancellationToken cancellationToken = default)
    {
        var content = JsonSerializer.Serialize(data);
        return await PostAsync(url, content, "application/json", configure, cancellationToken);
    }

    public async Task<TResponse?> PostJsonAsync<TRequest, TResponse>(string url, TRequest data,
        Action<HttpRequestOptions>? configure = null, CancellationToken cancellationToken = default)
    {
        var obj = await PostJsonAsync(url, data, configure, cancellationToken);
        obj.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<TResponse>(await obj.Content.ReadAsStringAsync(cancellationToken));
    }

    public async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, string> formData,
        Action<HttpRequestOptions>? configure = null, CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = CreateRequest(HttpMethod.Post, url, configure);
        httpRequestMessage.Content = new FormUrlEncodedContent(formData);
        return await Client.SendAsync(httpRequestMessage, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, string content,
        string contentType = "application/json", Action<HttpRequestOptions>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = CreateRequest(HttpMethod.Post, url, configure);
        httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, contentType);
        return await Client.SendAsync(httpRequestMessage, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync1(
        string url,
        string content,
        string contentType = "application/json",
        Action<HttpWrapperBuilder>? block = null,
        CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = CreateRequest(HttpMethod.Post, url, content, block);
        httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, contentType);
        return await Client.SendAsync(httpRequestMessage, cancellationToken);
    }


    public async Task<HttpResponseMessage> PostAsync(string url, byte[] content,
        string contentType = "application/octet-stream", Action<HttpRequestOptions>? configure = null,
        CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = CreateRequest(HttpMethod.Post, url, configure);
        httpRequestMessage.Content = new ByteArrayContent(content);
        httpRequestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return await Client.SendAsync(httpRequestMessage, cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, Action<HttpRequestOptions>? configure)
    {
        var httpRequestOptions = _defaultOptions.Clone();
        configure?.Invoke(httpRequestOptions);
        var requestUri = BuildUrl(url, httpRequestOptions.QueryParameters);
        var httpRequestMessage = new HttpRequestMessage(method, requestUri);
        if (httpRequestOptions.HttpVersion != null) httpRequestMessage.Version = httpRequestOptions.HttpVersion;
        foreach (var header in httpRequestOptions.Headers)
            httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return httpRequestMessage;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url, string content,
        Action<HttpWrapperBuilder>? block)
    {
        var httpRequestOptions = _defaultOptions.Clone();
        block?.Invoke(new HttpWrapperBuilder(_baseUrl, url, content));
        var requestUri = BuildUrl(url, httpRequestOptions.QueryParameters);
        var httpRequestMessage = new HttpRequestMessage(method, requestUri);
        if (httpRequestOptions.HttpVersion != null) httpRequestMessage.Version = httpRequestOptions.HttpVersion;
        foreach (var header in httpRequestOptions.Headers)
            httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        return httpRequestMessage;
    }

    private string BuildUrl(string url, Dictionary<string, string> queryParams)
    {
        var text = string.IsNullOrEmpty(_baseUrl) ? url : _baseUrl + "/" + url.TrimStart('/');
        if (queryParams.Count == 0) return text;
        var text2 = string.Join("&",
            queryParams.Select(kv => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value)));
        var text3 = text.Contains('?') ? "&" : "?";
        return text + text3 + text2;
    }

    private void ApplyDefaultOptions()
    {
        foreach (var header in _defaultOptions.Headers)
            Client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
    }
}