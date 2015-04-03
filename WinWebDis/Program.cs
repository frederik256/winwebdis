using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf.Nancy;
using Topshelf;
using Nancy;
using System.Threading;

namespace WinWebDis
{
    class Program
    {
        static int Main(string[] args)
        {
            return new EntryPoint().Main(args);
        }
    }

    class EntryPoint
    {
        public int Main(string[] args)
        {
            var host = HostFactory.New(x =>
            {
                //x.UseNLog();

                x.Service<RedisServiceCore>(s =>
                {
                    s.ConstructUsing(settings => new RedisServiceCore());
                    s.WhenStarted(service => service.Start());
                    s.WhenStopped(service => service.Stop());
                    s.WithNancyEndpoint(x, c =>
                    {
                        c.AddHost(port: 8080);
                    });
                });
                x.StartAutomatically();
                x.SetServiceName("WinWebDis");
                x.RunAsNetworkService();
            });

            var exitcode = host.Run();

            if (exitcode == TopshelfExitCode.Ok) return 0;

            return 1;
        }
    }




}
