using Serilog;
using Serilog.Debugging;
using ILogger = Serilog.ILogger;

namespace gRPCOnHttp3.Extensions;

public static class LoggerBuilderExtensions
{
        public static void Build(this LoggerConfiguration logger, IConfiguration configuration)
        {
            var serilogConfiguration = configuration.GetSection("Serilog");
            var seqUrl = serilogConfiguration["SeqUrl"];
            var appName = serilogConfiguration["AppName"];

            logger
                .Enrich.WithProperty("name", appName)
                .ReadFrom.Configuration(configuration);

            if (!string.IsNullOrWhiteSpace(seqUrl))
            {
                logger.WriteTo.Seq(
                    serverUrl: seqUrl
                );
                SelfLog.Enable(Console.Error);
            }
        }
}