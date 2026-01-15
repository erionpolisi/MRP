using Microsoft.Extensions.Configuration;

namespace MRP.Infrastructure
{
    public static class AppConfig
    {
        public static IConfiguration Configuration { get; }

        static AppConfig()
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets<AppMarker>()
                .AddEnvironmentVariables()
                .Build();
        }
    }

}
