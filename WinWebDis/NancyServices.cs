using System;
using Nancy;
using StackExchange.Redis;
using Nancy.IO;
using System.IO;

namespace WinWebDis
{
    public class CoreModule : NancyModule
    {
        public CoreModule()
        {
            Get["/status"] = _ => "I am alive! " + Guid.NewGuid().ToString();

            Post["/temp/{seconds:int}"] = _ =>
            {
                return SetTempKey(_);
            };

            Post["/set/{id:length(1, 250)}"] = _ =>
            {
                return SetKey(_);
            };

            Get["/get/{id:length(1, 250)}"] = _ =>
            {
                return GetKey(_);
            };
        }

        private static dynamic GetKey(dynamic _)
        {
            string id = _.id;
            IDatabase db = ServiceCore.RedisConnection.GetDatabase();
            RedisValue value = db.StringGet(id);
            
            if (!value.HasValue) return HttpStatusCode.NotFound;


            return new Response
            {             
                Contents = s => s.Write(value, 0, value.)
            };

            Response r = (Response)value;
            r.
            r.StatusCode = HttpStatusCode.OK; ;

            return r;
        }

        private dynamic SetTempKey(dynamic _)
        {
            int secondsTillExpiry = _.seconds;

            if (secondsTillExpiry < 0) throw new InvalidDataException("must have positive expiry value (in seconds).");

            var body = Request.Body.ReadAsString();
            var db = ServiceCore.RedisConnection.GetDatabase();

            var key = System.Guid.NewGuid().ToString("N");

            db.StringSet(key, body);
            db.KeyExpire(key, TimeSpan.FromSeconds(secondsTillExpiry), CommandFlags.FireAndForget);

            var r = (Response)key;
            r.StatusCode = HttpStatusCode.OK; ;

            return r;
        }

        private dynamic SetKey(dynamic _)
        {
            string id = _.id;
            var body = Request.Body.ReadAsString();
            var db = ServiceCore.RedisConnection.GetDatabase();

            db.StringSet(id, body);

            var r = (Response)id;
            r.StatusCode = HttpStatusCode.OK; ;

            return r;
        }
    }


    public class ServiceCore
    {
        private static RedisServiceState redis = new RedisServiceState();

        public static ConnectionMultiplexer RedisConnection
        {
            get { return redis.RedisConnection; }
        }

        public bool Start()
        {
            redis.Initialize();
            return true;
        }

        public bool Stop()
        {
            redis.Dispose();
            return true;
        }
    }

    public static class RequestBodyExtensions
    {
        public static string ReadAsString(this RequestStream requestStream)
        {
            using (var reader = new StreamReader(requestStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
