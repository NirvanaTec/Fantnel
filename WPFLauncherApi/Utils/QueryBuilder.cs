namespace WPFLauncherApi.Utils;

public class QueryBuilder
{
    private readonly Dictionary<string, string> _parameters = new();
    private string? _baseUrl;

    public QueryBuilder()
    {
    }

    public QueryBuilder(string urlOrQuery)
    {
        if (IsQueryStringOnly(urlOrQuery))
            ParseQueryString(urlOrQuery.TrimStart('?'));
        else
            ParseUrl(urlOrQuery);
    }

    public int Count => _parameters.Count;

    public static QueryBuilder FromDictionary(Dictionary<string, string> parameters)
    {
        var queryBuilder = new QueryBuilder();
        foreach (var parameter in parameters)
            queryBuilder._parameters[parameter.Key] = parameter.Value;
        return queryBuilder;
    }

    public static QueryBuilder FromObject(object obj)
    {
        var queryBuilder = new QueryBuilder();
        foreach (var property in obj.GetType().GetProperties())
        {
            var obj1 = property.GetValue(obj);
            if (obj1 != null)
                queryBuilder.Add(property.Name, obj1.ToString() ?? string.Empty);
        }

        return queryBuilder;
    }

    public static QueryBuilder FromParameters(string url)
    {
        var queryBuilder = new QueryBuilder();
        var queryStart = url.IndexOf('?');
        if (queryStart == -1) return queryBuilder;

        var queryString = url[(queryStart + 1)..];
        var parameters = queryString.Split('&');

        foreach (var param in parameters)
        {
            var parts = param.Split('=');
            if (parts.Length == 2) queryBuilder.Add(parts[0], Uri.UnescapeDataString(parts[1]));
        }

        return queryBuilder;
    }

    public QueryBuilder Add(string key, string value)
    {
        _parameters[key] = value;
        return this;
    }

    public QueryBuilder Add<T>(string key, T value)
    {
        if (value != null)
            _parameters[key] = value.ToString() ?? string.Empty;
        return this;
    }

    public QueryBuilder AddIf(bool condition, string key, string value)
    {
        if (condition)
            Add(key, value);
        return this;
    }

    public QueryBuilder AddIf(bool condition, string key, Func<string> valueFactory)
    {
        if (condition)
            Add(key, valueFactory());
        return this;
    }

    public QueryBuilder AddIfNotEmpty(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
            Add(key, value);
        return this;
    }

    public QueryBuilder AddRange(Dictionary<string, string> parameters)
    {
        foreach (var parameter in parameters)
            _parameters[parameter.Key] = parameter.Value;
        return this;
    }

    public QueryBuilder Merge(QueryBuilder other)
    {
        return AddRange(other._parameters);
    }

    public QueryBuilder Remove(string key)
    {
        _parameters.Remove(key);
        return this;
    }

    public QueryBuilder RemoveRange(params string[] keys)
    {
        foreach (var key in keys)
            _parameters.Remove(key);
        return this;
    }

    public QueryBuilder Clear()
    {
        _parameters.Clear();
        return this;
    }

    public string Get(string key)
    {
        return _parameters.GetValueOrDefault(key) ?? throw new Exception("Parameter not found");
    }

    public string GetOrDefault(string key, string defaultValue)
    {
        return _parameters.GetValueOrDefault(key, defaultValue);
    }

    public bool Contains(string key)
    {
        return _parameters.ContainsKey(key);
    }

    public Dictionary<string, string> GetAll()
    {
        return new Dictionary<string, string>(_parameters);
    }

    public QueryBuilder WithBaseUrl(string url)
    {
        _baseUrl = url.Split('?')[0];
        return this;
    }

    public string BuildQueryString()
    {
        return _parameters.Count == 0
            ? string.Empty
            : string.Join("&",
                _parameters.Where((Func<KeyValuePair<string, string>, bool>)(p => !string.IsNullOrEmpty(p.Value)))
                    .Select((Func<KeyValuePair<string, string>, string>)(p =>
                        $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}")));
    }

    public string BuildUrl()
    {
        if (string.IsNullOrEmpty(_baseUrl))
            throw new InvalidOperationException(
                "Base URL is not set. Call WithBaseUrl() first or use BuildQueryString().");
        var str = BuildQueryString();
        return string.IsNullOrEmpty(str) ? _baseUrl : _baseUrl + (_baseUrl.Contains('?') ? "&" : "?") + str;
    }

    public static string BuildUrl(string baseUrl, Dictionary<string, string> parameters)
    {
        return new QueryBuilder().WithBaseUrl(baseUrl).AddRange(parameters).BuildUrl();
    }

    private static bool IsQueryStringOnly(string input)
    {
        if (input.StartsWith('?'))
            return true;
        return input.Contains('=') && !input.Contains("://");
    }

    private void ParseUrl(string url)
    {
        if (!url.Contains('?'))
        {
            _baseUrl = url;
        }
        else
        {
            var strArray = url.Split('?', 2);
            _baseUrl = strArray[0];
            ParseQueryString(strArray[1]);
        }
    }

    private void ParseQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString))
            return;
        foreach (var str in queryString.Split('&'))
            if (!string.IsNullOrWhiteSpace(str))
            {
                var strArray = str.Split('=', 2);
                if (strArray.Length == 2)
                    _parameters[Uri.UnescapeDataString(strArray[0])] = Uri.UnescapeDataString(strArray[1]);
            }
    }

    public QueryBuilder Clone()
    {
        var queryBuilder = new QueryBuilder
        {
            _baseUrl = _baseUrl
        };
        foreach (var parameter in _parameters)
            queryBuilder._parameters[parameter.Key] = parameter.Value;
        return queryBuilder;
    }

    public override string ToString()
    {
        return BuildQueryString();
    }
}