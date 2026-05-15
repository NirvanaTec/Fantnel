using System;
using System.Collections.Generic;

namespace Nirvana.WPFLauncher.Http;

public class HttpRequestOptions {
    public Version? HttpVersion;
    public Dictionary<string, string> Headers { get; } = new();

    public Dictionary<string, string> QueryParameters { get; } = new();

    private void AddHeader(string key, string value)
    {
        Headers[key] = value;
    }

    public void AddHeaders(Dictionary<string, string> headers)
    {
        foreach (var header in headers) {
            Headers[header.Key] = header.Value;
        }
    }

    public void UserAgent(string userAgent)
    {
        AddHeader("User-Agent", userAgent);
    }

    internal HttpRequestOptions Clone()
    {
        var httpRequestOptions = new HttpRequestOptions {
            HttpVersion = HttpVersion
        };
        foreach (var header in Headers){
            httpRequestOptions.Headers[header.Key] = header.Value;
        }
        foreach (var queryParameter in QueryParameters) {
            httpRequestOptions.QueryParameters[queryParameter.Key] = queryParameter.Value;
        }
        return httpRequestOptions;
    }
}