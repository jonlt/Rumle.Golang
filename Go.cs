using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rumle.Golang
{
    public static class Go
    {
        public static void Run(Action func)
        {
            var t = new Thread(() => func());
            t.IsBackground = true;
            t.Start();
        }
    }
}
