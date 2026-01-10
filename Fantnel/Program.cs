using System.Text;
using Codexus.Game.Launcher.Utils;
using Fantnel.Servlet;
using Microsoft.Extensions.FileProviders;
using NirvanaPublic;
using NirvanaPublic.Utils;
using Serilog;

namespace Fantnel;

public static class Program
{
    public static void Main(string[] args)
    {
        // Fantnel 日志初始化
        LogoInit();

        // 检查更新
        Log.Information("{Path}", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources"));
        UpdateTools.CheckUpdate(args).Wait();

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

        // 没有配置时，默认监听 13521 端口
        if (app.Urls.Count == 0)
            // 监听未被占用的端口
            app.Urls.Add("http://localhost:" + Tools.GetUnusedPort(13521));

        // 配置 HTTP 请求管道。
        // if (app.Environment.IsDevelopment()) 
        app.MapOpenApi();

        // 用户可能未配置证书，所以不启用HTTPS重定向
        // app.UseHttpsRedirection();

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

        // 处理 404 错误，将请求重定向到首页
        app.Use(async (context, next) =>
        {
            await next();
            if (context.Response.StatusCode == 404)
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(HomeController.GetIndexHtml());
            }
        });

        // 在应用启动前清空控制台并输出访问地址
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            // Fantnel 日志初始化
            LogoInit();

            // 分割显示多个URL
            Log.Information("访问地址:");
            foreach (var url in app.Urls) Log.Information("  {Url}", url);

            Log.Information("本项目遵循 GNU GPL 3.0 协议开源");
            Log.Information("------");
            // Log.Information("静态文件: {Path}", resourcesPath);
            Log.Information("官方网址: https://npyyds.top/");
            Log.Information("最终解释权归于 涅槃科技 所有!");
            Log.Information("---------- 涅槃科技 & Codexus (OpenSDK) ----------");

            // Fantnel 初始化
            InitProgram.NelInit();

            Log.Information("{Path}", resourcesPath);
            Log.Information("Java: {Path}", PathUtil.JavaPath);
        });

        app.Run();
    }

    private static void LogoInit()
    {
        InitProgram.LogoInit();
        Log.Information("----- Fantnel -----");
        Log.Information("应用启动成功！");
        Log.Information("版本: {ver}", PublicProgram.Version);
    }
}