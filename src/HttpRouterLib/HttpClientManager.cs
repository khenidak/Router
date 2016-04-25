using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HttpRouterLib
{
    // Manages a set of HttpClient (s) 
    class HttpClientManager
    {
        private ConcurrentDictionary<string, HttpClient> mClients = new ConcurrentDictionary<string, HttpClient>();

        public HttpClient getHttpClient(string host)
        {
            // if you want to do any funcky business with the client then you can do it here

           
            return mClients.GetOrAdd(host, (key) => new HttpClient());
        } 
    }
}
