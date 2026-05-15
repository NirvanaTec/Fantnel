using System;
using System.Collections.Generic;
using System.Linq;

namespace Nirvana.WPFLauncher.Utils;

public class QueryBuilder {
    private readonly Dictionary<string, string> _parameters = new();

    public QueryBuilder()
    {
        
    }

    public QueryBuilder(string queryString)
    {
        ParseQueryString(queryString);
    }

    public static QueryBuilder FromParameters(string url)
    {
        var queryBuilder = new QueryBuilder();
        var queryStart = url.IndexOf('?');
        if (queryStart == -1) {
            return queryBuilder;
        }

        var queryString = url[(queryStart + 1)..];
        var parameters = queryString.Split('&');

        foreach (var param in parameters) {
            var parts = param.Split('=');
            if (parts.Length == 2) {
                queryBuilder.Add(parts[0], Uri.UnescapeDataString(parts[1]));
            }
        }

        return queryBuilder;
    }

    public QueryBuilder Add(string key, string value)
    {
        _parameters[key] = value;
        return this;
    }

    public string Get(string key)
    {
        return _parameters.GetValueOrDefault(key) ?? throw new Exception("Parameter not found");
    }

    public Dictionary<string, string> GetAll()
    {
        return new Dictionary<string, string>(_parameters);
    }

    public string BuildQueryString()
    {
        return _parameters.Count == 0 ? string.Empty : string.Join("&", _parameters.Where(p => !string.IsNullOrEmpty(p.Value)).Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }

    private void ParseQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString)) {
            return;
        }

        foreach (var str in queryString.Split('&')) {
            if (string.IsNullOrWhiteSpace(str)) {
                continue;
            }

            var strArray = str.Split('=', 2);
            if (strArray.Length == 2) {
                _parameters[Uri.UnescapeDataString(strArray[0])] = Uri.UnescapeDataString(strArray[1]);
            }
        }
    }

    public override string ToString()
    {
        return BuildQueryString();
    }
}