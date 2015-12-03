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
    /// Configures HttpRoutingContext based on Owin IDictionary<string, string[]> envrionment:
    /// 1- copies method from Owin Ctx.
    /// 2- Copies headers from Owin Ctx.
    /// </summary>
    public class HttpFromOwinReqCtxMatcher : MatcherBase
    {
        public HttpFromOwinReqCtxMatcher()
        {

        }

        public override async Task<bool> MatchAsync(RoutingContextBase routingContext,
                                              string sAddress,
                                              IDictionary<string, object> Context,
                                              Stream Body)
        {
            // validate that the in Ctx actually has owin
            if (null == (Context["owin.RequestScheme"] as string))
                throw new InvalidOperationException("Context does not contain Owin");

            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var httpCtx = routingContext as HttpRoutingContext;

            // copy http method
            var OwinMethodString = Context["owin.RequestMethod"] as string;
            httpCtx.Method = new HttpMethod(OwinMethodString);

            // copy headers
            var OwinRequestHeader = Context["owin.RequestHeaders"] as IDictionary<string, string[]>;
            foreach (var header in OwinRequestHeader)
                httpCtx.Headers.Add(header.Key, header.Value);

            return true;
        }
    }
}
