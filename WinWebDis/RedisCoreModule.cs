﻿using System;
using Nancy;
using Nancy.IO;
using System.IO;
using StackExchange.Redis;

namespace WinWebDis
{
    public class RedisCoreModule : NancyModule
    {
        private const int POST_LIMIT = 64 * 1024 * 1024;

        public RedisCoreModule()
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
            IDatabase db = RedisServiceCore.RedisConnection.GetDatabase();
            byte[] value = db.StringGet(id);

            return new Response
            {
                Contents = s => s.Write(value, 0, value.Length),
                StatusCode = HttpStatusCode.OK
            };
        }

        private dynamic SetTempKey(dynamic _)
        {
            int secondsTillExpiry = _.seconds;

            if (secondsTillExpiry < 0) throw new InvalidDataException("must have positive expiry value (in seconds).");

            byte[] body = Request.Body.ReadAllBytes(POST_LIMIT);
            var db = RedisServiceCore.RedisConnection.GetDatabase();

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
}
