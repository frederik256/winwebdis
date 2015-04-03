using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Collections.Generic;

namespace WinWebDis
{
    public class RedisServiceCore
    {
        private static RedisConnectionHelper redisConnection = new RedisConnectionHelper();
        private RedisSpawner redisProcess = new RedisSpawner();

        public static ConnectionMultiplexer RedisConnection
        {
            get { return redisConnection.RedisConnection; }
        }

        public bool Start()
        {
            redisProcess.StartRedis();
            redisConnection.Initialize();
            return true;
        }

        public bool Stop()
        {
            redisConnection.Dispose();
            return true;
        }
    }

    internal class RedisConnectionHelper : IDisposable
    {
        public ConnectionMultiplexer RedisConnection { get; set; }
        public string RedisConnectionString = "localhost:7777";

        private bool disposed;

        public void Initialize()
        {
            ConfigurationOptions config = new ConfigurationOptions() { ConnectRetry = 50 };
            config.EndPoints.Add(RedisConnectionString);

            RedisConnection = ConnectionMultiplexer.Connect(config);
        }

        public void ShutDownRedisServer()
        {
            ConfigurationOptions config = new ConfigurationOptions() { ConnectRetry = 5, AllowAdmin = true };
            config.EndPoints.Add(RedisConnectionString);
            ConnectionMultiplexer adminConnection = ConnectionMultiplexer.Connect(config);

            adminConnection.GetServer(RedisConnectionString).Shutdown(ShutdownMode.Default);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (RedisConnection != null)
                {
                    RedisConnection.Dispose();
                    RedisConnection = null;
                }

                ShutDownRedisServer();
            }
            disposed = true;
        }
    }
}
