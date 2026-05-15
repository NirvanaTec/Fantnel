using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nirvana.WPFLauncher.Http;

public class HttpWrapper : IDisposable {
    private readonly string _baseUrl;

    private readonly HttpRequestOptions _defaultOptions;

    public HttpWrapper(string baseUrl = "", Action<HttpRequestOptions>? configureDefaults = null, HttpClientHandler? handler = null)
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

    public async Task<HttpResponseMessage> GetAsync(string url, Action<HttpRequestOptions>? configure = null, CancellationToken cancellationToken = default)
    {
        var request = CreateRequest(HttpMethod.Get, url, configure);
        return await Client.SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, string content, string contentType = "application/json", Action<HttpRequestOptions>? configure = null, CancellationToken cancellationToken = default)
    {
        var httpRequestMessage = CreateRequest(HttpMethod.Post, url, configure);
        httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, contentType);
        return await Client.SendAsync(httpRequestMessage, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsync(string url, byte[] content, string contentType = "application/octet-stream", Action<HttpRequestOptions>? configure = null, CancellationToken cancellationToken = default)
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
        if (httpRequestOptions.HttpVersion != null) {
            httpRequestMessage.Version = httpRequestOptions.HttpVersion;
        }

        foreach (var header in httpRequestOptions.Headers) {
            httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return httpRequestMessage;
    }

    private string BuildUrl(string url, Dictionary<string, string> queryParams)
    {
        var text = string.IsNullOrEmpty(_baseUrl) ? url : _baseUrl + "/" + url.TrimStart('/');
        if (queryParams.Count == 0) {
            return text;
        }

        var text2 = string.Join("&", queryParams.Select(kv => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value)));
        var text3 = text.Contains('?') ? "&" : "?";
        return text + text3 + text2;
    }

    private void ApplyDefaultOptions()
    {
        foreach (var header in _defaultOptions.Headers) {
            Client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}