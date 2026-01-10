using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NirvanaPublic.Entities.Nirvana;
using NirvanaPublic.Utils;
using WPFLauncherApi.Entities;
using WPFLauncherApi.Utils.CodeTools;

namespace Fantnel.Servlet;

public class WebApiExceptionFilter : ExceptionFilterAttribute
{
    // 异常处理
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        EntityResponse<object>? response; // 信息
        var array = new JsonArray(); // 异常追踪
        if (context.Exception is ErrorCodeException errorCodeException)
        {
            response = errorCodeException.GetJson();
        }
        else
        {
            response = new EntityResponse<object>
            {
                Code = -1,
                Msg = Tools.GetMessage(context.Exception)
            };
            var stack = GetStackTrace(context.Exception);
            if (stack != null) array.Add(stack);
        }

        var index = array.Count;
        var stackTrace = new StackTrace(context.Exception, true);
        foreach (var frame in stackTrace.GetFrames())
        {
            var stackTraceFrame = new EntityStackTrace(frame);
            if (stackTraceFrame.IsIgnore()) continue;
            if (index++ > 8) break;
            array.Add(stackTraceFrame.ToJsonDocument());
        }

        response.Data = array;
        context.Result = new JsonResult(response);
        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }

    private static object? GetStackTrace(Exception exception)
    {
        switch (exception)
        {
            case AggregateException aggregateException:
            {
                var jsonArray = new JsonArray();
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    var stackTrace = GetStackTrace(innerException);
                    if (stackTrace != null) jsonArray.Add(stackTrace);
                }

                return jsonArray.Count == 0 ? null : jsonArray;
            }
            case EntityX19Exception entityX19Exception:
                return entityX19Exception.Data;
            case ErrorCodeException errorCodeException:
                return errorCodeException.Entity.Data;
            default:
                return null;
        }
    }
}