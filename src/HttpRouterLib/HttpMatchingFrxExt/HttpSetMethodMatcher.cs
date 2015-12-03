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
    /// sets http method on HttpRoutingContext
    /// </summary>
    public class HttpSetMethodMatcher : MatcherBase
    {
        public HttpMethod TargetMethod { get; set; } = HttpMethod.Get;
        public HttpSetMethodMatcher()
        {

        }

        public HttpSetMethodMatcher(HttpMethod method)
        {
            

            TargetMethod = method;
        }


        public override async Task<bool> MatchAsync(RoutingContextBase routingContext,
                                              string sAddress,
                                              IDictionary<string, object> Context,
                                              Stream Body)
        {
            if (null == TargetMethod)
                throw new ArgumentNullException("TargetMethod");

            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var httpCtx = routingContext as HttpRoutingContext;
            httpCtx.Method = this.TargetMethod;

            return true;
        }
    }
}
