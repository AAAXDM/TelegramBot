using Npgsql;

namespace WebApplication1
{
    public static class ConnectionHelper
    {
        public static string? GetConnectionString(IConfiguration configuration)
        {
            var dataBaseUrl = configuration["DatabaseUrl"];
            return  BuildConnectionString(dataBaseUrl);
        }

        static string BuildConnectionString(string databaseUrl)
        {
            var databaseUri = new Uri(databaseUrl);
            var userInfo = databaseUri.UserInfo.Split(':');

            var builder = new NpgsqlConnectionStringBuilder
            {
                Host = databaseUri.Host,
                Port = databaseUri.Port,
                Username = userInfo[0],
                Password = userInfo[1],
                Database = databaseUri.LocalPath.TrimStart('/')
            };

            return builder.ToString();
        }
    }
}