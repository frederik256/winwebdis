using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WinWebDis
{
    public class RedisSpawner
    {
        public Process _proc;

        public void StartRedis()
        {
            _proc = new Process();            
            _proc.StartInfo.FileName = @".\Redis\redis-server.exe";
            _proc.StartInfo.RedirectStandardError = true;
            _proc.StartInfo.RedirectStandardOutput = true;
            _proc.StartInfo.RedirectStandardInput = true;
            _proc.StartInfo.UseShellExecute = false;
            _proc.StartInfo.Arguments = @".\Redis\redis.windows.conf";
            _proc.Start();                      
        }

        public void KillRedis() // just in case... 
        {            
            _proc.Kill();
        }
    }
}
