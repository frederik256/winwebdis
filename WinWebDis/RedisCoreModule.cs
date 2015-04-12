using System;
using Nancy;
using Nancy.IO;
using System.IO;
using StackExchange.Redis;

namespace WinWebDis
{
    public class RedisCoreModule : NancyModule
    {
        private const int POST_LIMIT = 64 * 1024 * 1024;
        private const int MAX_REDIS_KEY_LENGTH = 1250;
        private const int MAX_GUID_KEY_LENGTH = MAX_REDIS_KEY_LENGTH - 32 - 1; // assume "N" format for guid and a separator.  
        private const string NAMESPACING_SEPARATOR = "_";

        public RedisCoreModule()
        {
            this.EnableCors();

            Get["/status"] = _ => "I am alive! " + Guid.NewGuid().ToString();


            Post["/tempns/{nameSpace:length(1," + MAX_GUID_KEY_LENGTH + ")}/{seconds:int}"] = _ =>
            {
                return SetTempKey(_.seconds, _.nameSpace);
            };

            Post["/temp/{seconds:int}"] = _ =>
            {
                return SetTempKey(_.seconds);
            };


            Post["/set/{id:length(1, 250)}"] = _ =>
            {
                return SetKey(_.id);
            };

            Get["/get/{id:length(1, 250)}"] = _ =>
            {
                return GetKey(_.id);
            };
        }

        private static dynamic GetKey(string id)
        {
            IDatabase db = RedisServiceCore.RedisConnection.GetDatabase();
            byte[] value = db.StringGet(id);

            return new Response
            {
                Contents = s => s.Write(value, 0, value.Length),
                StatusCode = HttpStatusCode.OK
            };
        }

        private dynamic SetTempKey(int secondsTillExpiry, string keyprefix = "")
        {
            if (secondsTillExpiry < 0) throw new InvalidDataException("must have positive expiry value (in seconds).");

            byte[] body = Request.Body.ReadAllBytes(POST_LIMIT);

            string bodyAsString_debug = System.Text.Encoding.UTF8.GetString(body, 0, body.Length);

            var db = RedisServiceCore.RedisConnection.GetDatabase();
            var guidStr = System.Guid.NewGuid().ToString("N");

            var key = (keyprefix == "") ? guidStr : keyprefix + ":" + guidStr;

            db.StringSet(key, body);
            db.KeyExpire(key, TimeSpan.FromSeconds(secondsTillExpiry), CommandFlags.FireAndForget);

            var r = (Response)key;
            r.StatusCode = HttpStatusCode.OK; ;
           
            return r;
        }

        private dynamic SetKey(string id)
        {           
            byte[] body = Request.Body.ReadAllBytes(POST_LIMIT);
            var db = RedisServiceCore.RedisConnection.GetDatabase();

            db.StringSet(id, body);

            var r = (Response)id;
            r.StatusCode = HttpStatusCode.OK; ;

            return r;
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

        public static byte[] ReadAllBytes(this RequestStream requestStream, long maxLength = long.MaxValue)
        {
            long length = requestStream.Length;

            if (length > maxLength) throw new InvalidDataException("Maximum length for stream exceeded.");

            byte[] bytes = new byte[requestStream.Length];
            requestStream.Read(bytes, 0, (int)length);
            return bytes;
        }
    }

    public static class NancyExtensions
    {
        public static void EnableCors(this NancyModule module)
        {
            module.After.AddItemToEndOfPipeline(x =>
            {
                x.Response.WithHeader("Access-Control-Allow-Origin", "*");
            });
        }
    }
}
