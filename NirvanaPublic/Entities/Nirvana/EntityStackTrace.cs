using System.Diagnostics;
using System.Text.Json;

namespace NirvanaPublic.Entities.Nirvana;

public class EntityStackTrace
{
    public EntityStackTrace(StackFrame stackTrace)
    {
        var method = stackTrace.GetMethod();
        if (method == null) throw new Exception("Method Not Found");
        Method = method.Name;
        File = stackTrace.GetFileName() ?? string.Empty;
        Line = stackTrace.GetFileLineNumber();
    }

    public string Method { get; set; }
    public string File { get; set; }
    public int Line { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }

    public JsonDocument ToJsonDocument()
    {
        return JsonDocument.Parse(ToString());
    }

    // 是否是不重要的
    public bool IsIgnore()
    {
        return (string.IsNullOrEmpty(File) && Line == 0) || Method.Length < 5;
    }
}