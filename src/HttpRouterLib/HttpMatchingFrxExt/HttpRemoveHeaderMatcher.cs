using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RouterLib;

namespace HttpRouterLib
{
    /// <summary>
    /// Removes a header from HttpRoutingContext.Headers
    /// </summary>
    public class HttpRemoveHeaderMatcher : MatcherBase
    {

        public string HeaderName { get; set; }
        
       

        public HttpRemoveHeaderMatcher() : this(null)
        {

        }






        public HttpRemoveHeaderMatcher(string headerName)
        {
            HeaderName = headerName;
        }



        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                           string sAddress,
                                           IDictionary<string, object> Context,
                                           Stream Body)
        {

            if (string.IsNullOrEmpty(HeaderName))
                throw new ArgumentNullException("HeaderName");


            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var httpCtx = routingContext as HttpRoutingContext;

            httpCtx.Headers.Remove(HeaderName);

            return true;
        }





    }
}
