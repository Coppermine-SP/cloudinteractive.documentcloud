using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.Metadata;
using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace cloudinteractive.documentcloud
{
    public static class RequestManagement
    {
        private static ILogger? _logger;
        private static ConnectionMultiplexer _redis;
        private static IDatabase _db;
        private static IConfiguration _config;
        public static void Init(IConfiguration config)
        {
            _logger = ApplicationLogging.CreateLogger("RequestManagement");
            _logger.LogInformation("RequestManagement init..");
            _config = config;

            string server = config.GetSection("RedisServer").Value ?? "127.0.0.1:6379";
            _logger.LogInformation($"Connect to Redis server : {server}...");
            _redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                EndPoints = { server }
            });
            _db = _redis.GetDatabase();

            var pong = _db.Ping();
            _logger.LogInformation($"Successfully connected. (took {pong}sec.)");

        }

        public enum RequestStatus {Queued = 0, Exporting = 1, Processing = 2, Complete = 3, Error = 4}

        private static string _getStatusKey(string requestId)
        {
            return $"{requestId}:status";
        }

        private static string _getResultKey(string requestId)
        {
            return $"{requestId}:result";
        }

        public static void SetRequestStatus(string requestId, RequestStatus status)
        {
            _db.StringSet(_getStatusKey(requestId), ((int)status).ToString(), TimeSpan.FromMinutes(10));
        }

        public static void SetRequestResult(string requestId, string result)
        {
            _db.StringSet(_getResultKey(requestId), result, TimeSpan.FromMinutes(10));
        }

        public static RequestStatus? GetRequestStatus(string requestId)
        {
            string? status = _db.StringGet(_getStatusKey(requestId));

            if (status is null) return null;
            return (RequestStatus)Convert.ToInt32(status);
        }

        public static string? GetRequestResult(string requestId)
        {
            return _db.StringGet(_getResultKey(requestId));
        }
    }
}
