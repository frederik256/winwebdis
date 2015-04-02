using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace WinWebDis
{
    internal class RedisServiceState : IDisposable
    {
        public ConnectionMultiplexer RedisConnection { get; set; }
        public string RedisConnectionString = "localhost:7777";

        private bool disposed;

        public void Initialize()
        {
            RedisConnection = ConnectionMultiplexer.Connect(RedisConnectionString);
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
            }
            disposed = true;
        }
    }

}
