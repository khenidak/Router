using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterTests
{
    class TestWebServer
    {
        private IDisposable mWebServer;
        private string mAddress;

        public  void Start<T>(string sListeningAddress )
        {
            mAddress = sListeningAddress;

            var startupOptions = new StartOptions();
            mWebServer = WebApp.Start<T>(mAddress);

            Trace.WriteLine(string.Format("Test Web Server: Started On {0} with Type {1}", 
                                           mAddress, 
                                           typeof(T).ToString()));
        }

        public void Stop()
        {
            if (null != mWebServer)
            { 
                mWebServer.Dispose();

                Trace.WriteLine(string.Format("Test Web Server on {0} stopped",
                                           mAddress));
            }

        }
    }
}
