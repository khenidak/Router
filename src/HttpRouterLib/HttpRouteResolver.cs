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
    /// http resolver used by the HttpRouter
    /// </summary>
    public class HttpRouteResolver : ResolverBase
    {
        public override async Task<RoutingContextBase> ResolveAsync(string sAddress, 
                                                                    IDictionary<string, object> Context, 
                                                                    Stream Body)
        {
            var re = new HttpRoutingContext(this, sAddress, Context, Body);
            return await base.ResolveAsync(re, sAddress, Context, Body);
        }
    }
}
