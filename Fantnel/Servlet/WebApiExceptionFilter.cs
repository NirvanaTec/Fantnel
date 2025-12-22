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
        context.Result = new JsonResult(response);
        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}