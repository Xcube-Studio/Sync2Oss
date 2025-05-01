using Aliyun.OSS;
using Aliyun.OSS.Common;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Sync2Oss.Services;

namespace Sync2Oss;

public class Program
{
    public static async Task Main(string[] args)
    {
        ServiceCollection services = new();

        services.AddSingleton<OssClient>(p =>
        {
            var dict = p.GetRequiredService<Dictionary<string, string>>();

            return new OssClient(dict["endpoint"], dict["accessKeyId"], dict["accessKeySecret"],
                new ClientConfiguration() { SignatureVersion = SignatureVersion.V4 });
        });

        services.AddSingleton(args);
        services.AddSingleton<Dictionary<string, string>>(_ => []);
        services.AddSingleton<AliyunOssService>();
        services.AddSingleton<ConsoleService>();

        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog());
        services.AddSerilog(configure =>
        {
            configure.WriteTo.Logger(l =>
            {
                l.WriteTo.Console(
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}][{Level:u3}] <{SourceContext}>: {Message:lj}{NewLine}{Exception}");
            });
        });

        IServiceProvider provider = services.BuildServiceProvider();

        await provider.GetService<ConsoleService>()!.ExecuteAsync();
    }
}
