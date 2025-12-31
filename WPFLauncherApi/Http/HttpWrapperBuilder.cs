namespace WPFLauncherApi.Http;

public class HttpWrapperBuilder(string baseUrl, string url, string body)
{
    private readonly Dictionary<string, string> _headers = new();

    public string BaseUrl => baseUrl;

    public string Url => url;

    public string Body => body;

    public HttpWrapperBuilder AddHeader(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
            _headers.Add(header.Key, header.Value);
        return this;
    }

    public HttpWrapperBuilder AddHeader(string key, string value)
    {
        _headers.Add(key, value);
        return this;
    }

    public HttpWrapperBuilder UserAgent(string userAgent)
    {
        _headers.Add("User-Agent", userAgent);
        return this;
    }

    public Dictionary<string, string> GetHeaders()
    {
        return _headers;
    }
}