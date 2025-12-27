using System.Diagnostics;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NirvanaPublic.Entities.Nirvana;
using NirvanaPublic.Utils.ViewLogger;

namespace Fantnel.Servlet;

public class WebApiExceptionFilter : ExceptionFilterAttribute
{
    // 异常处理
    public override Task OnExceptionAsync(ExceptionContext context)
    {
        EntityResponse<object>? response;
        if (context.Exception is Code.ErrorCodeException errorCodeException)
            response = errorCodeException.GetJson();
        else
            response = new EntityResponse<object>
            {
                Code = -1,
                Msg = context.Exception.Message
            };

        // 异常详情
        var array = new JsonArray();
        var stackTrace = new StackTrace(context.Exception, true);
        var index = 0;
        foreach (var frame in stackTrace.GetFrames())
        {
            if (index++ > 10) break;
            array.Add(new EntityStackTrace(frame).ToJsonDocument());
        }

        // 聚合异常，合并异常信息
        if (context.Exception is AggregateException aggregateException)
        {
            response.Msg = "";
            foreach (var innerException in aggregateException.InnerExceptions)
                if (context.Exception is Code.ErrorCodeException errorCodeException1)
                    response.Msg += errorCodeException1.Entity.Msg + ", ";
                else
                    response.Msg += innerException.Message + ", ";

            response.Msg = response.Msg.TrimEnd(',', ' ');
        }

        response.Data = array;
        context.Result = new JsonResult(response);
        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}