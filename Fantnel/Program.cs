using System.Text;
using Fantnel.Servlet;
using Microsoft.Extensions.FileProviders;
using NirvanaPublic;
using NirvanaPublic.Entities;
using Serilog;

namespace Fantnel;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 设置默认编码为UTF-8
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Console.OutputEncoding = Encoding.UTF8;

        // 将服务添加到容器中。
        builder.Services.AddOpenApi();
        builder.Services.AddControllers(options =>
        {
            // 添加全局异常过滤器
            options.Filters.Add<WebApiExceptionFilter>();
        });

        var app = builder.Build();

        // 配置 HTTP 请求管道。
        if (app.Environment.IsDevelopment()) app.MapOpenApi();

        app.UseHttpsRedirection();

        // 获取运行目录路径
        var resourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "static");

        // 启用静态文件服务，从运行目录的 resources/static 目录提供文件
        if (Directory.Exists(resourcesPath))
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(resourcesPath),
                RequestPath = ""
            });

        app.MapControllers();

        // 在应用启动前清空控制台并输出访问地址
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            // Fantnel 日志初始化
            PublicProgram.LogoInit();

            Log.Information("----- Fantnel -----");
            Log.Information("应用启动成功！");
            Log.Information("版本: {ver}", InfoManager.FantnelVersion);

            // 分割显示多个URL
            var urls = app.Configuration["ASPNETCORE_URLS"]?.Split(';');
            if (urls != null)
            {
                Log.Information("访问地址:");
                foreach (var url in urls) Log.Information("  {Url}", url);
            }
            else
            {
                Log.Information("访问地址: 未配置");
            }

            Log.Information("本项目遵循 GNU GPL 3.0 协议开源");
            Log.Information("------");
            Log.Information("官方网址: https://npyyds.top/");
            Log.Information("最终解释权归于 涅槃科技 所有!");
            Log.Information("---------- 涅槃科技 & Codexus.OpenSDK ----------");

            // Fantnel 初始化
            PublicProgram.NelInit().Wait();
        });

        app.Run();
    }
}