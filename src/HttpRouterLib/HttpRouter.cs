using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using RouterLib;

namespace HttpRouterLib
{ 
    /// <summary>
    /// Http router, provides no additional capiabilities to
    /// the base router other than ensuring that a resolver is always provided.
    /// </summary>
    public class HttpRouter : Router<HttpRoutingResult, HttpRouteResolver>
    {
        
        // http router can work with http resolver 
        // as long as you the wiring is done right you should be cool
        public HttpRouter(HttpRouteResolver customResolver)  
        {
            base.mResolver = customResolver;
        }

        



        

    }
}
