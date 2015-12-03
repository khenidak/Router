using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Linq;

using System.Net.Http;
using System.Collections.Generic;

using RouterLib;
using HttpRouterLib;

namespace RouterTests
{
    [TestClass]
    public partial class RouterTestSuit
    {
        private TestWebServer mApiServer01;
        private TestWebServer mApiServer02;
        private TestWebServer mApiServer03;


        // Socket Server 
        public static string OwinSocketRouterSrvAddress = "http://localhost:8000/";
        public static string OwinSocketRouterSrvAddressExternal = "ws://localhost:8000/";



        // Owin Router
        public static string OwinRouterSrvAddress = "http://localhost:9000/";
        public static Uri OwinRouterSrvAddressUri = new Uri(OwinRouterSrvAddress);
        public static string OwinRouterSrvHostNamePort = string.Concat(OwinRouterSrvAddressUri.Host, ":", OwinRouterSrvAddressUri.Port);



        // test 3 backend servers
        public static string srv01Address = "http://localhost:9001/";
        public static string srv02Address = "http://localhost:9002/";
        public static string srv03Address = "http://localhost:9003/";


        public static Uri srv01Uri = new Uri(srv01Address);
        public static string Srv01HostNamePort = string.Concat(srv01Uri.Host, ":", srv01Uri.Port);

        public static Uri srv02Uri = new Uri(srv02Address);
        public static string Srv02HostNamePort = string.Concat(srv02Uri.Host, ":", srv02Uri.Port);

        public static Uri srv03Uri = new Uri(srv03Address);
        public static string Srv03HostNamePort = string.Concat(srv03Uri.Host, ":", srv03Uri.Port);



        [TestInitialize]
        public void InitTests()
        {

            Trace.WriteLine(" ***** #define ENABLE_LONGRUNNING_TESTS for long running tests **** ");

            mApiServer01 = new TestWebServer();
            mApiServer02 = new TestWebServer();
            mApiServer03 = new TestWebServer();

            mApiServer01.Start<ApiAppStartUp>(srv01Address);
            mApiServer02.Start<ApiAppStartUp>(srv02Address);
            mApiServer03.Start<ApiAppStartUp>(srv03Address);

            
        }
        

        [TestCleanup]
        public void TestCleanUp()
        {
            if (null != mApiServer01)
                mApiServer01.Stop();

            if (null != mApiServer02)
                mApiServer02.Stop();

            if (null != mApiServer03)
                mApiServer03.Stop();
        }

        
    }
}
