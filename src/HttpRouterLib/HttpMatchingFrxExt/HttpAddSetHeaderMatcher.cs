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
    /// adds or sets header in HttpRoutingContext.Headers
    /// </summary>
    public class HttpAddSetHeaderMatcher : MatcherBase
    {

        public string HeaderName { get; set; }
        
        public List<string> HeaderValues { get; set; } = new List<string>();

        public HttpAddSetHeaderMatcher() : this(null, headerValues: null)
        {

        }

        public HttpAddSetHeaderMatcher(string headerName, string headerValue) : this(headerName, new List<string>() { headerValue })
        {

        }





        public HttpAddSetHeaderMatcher(string headerName, List<string> headerValues)
        {
            HeaderName = headerName;
            HeaderValues = headerValues;
        }



        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                           string sAddress,
                                           IDictionary<string, object> Context,
                                           Stream Body)
        {

            if (string.IsNullOrEmpty(HeaderName))
                throw new ArgumentNullException("HeaderName");


            if (null == HeaderValues)
                throw new ArgumentNullException("HeaderValues");

            // don't validate against empty string, because user may want to add 
            // an empty header value

            var anyNull = HeaderValues.Where(v => null== v);
            if (0 != anyNull.Count())
                throw new ArgumentNullException("header values contains one or more null values");

            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var httpCtx = routingContext as HttpRoutingContext;

            if(httpCtx.Headers.ContainsKey(HeaderName))
                httpCtx.Headers[HeaderName]=  HeaderValues;
            else
                httpCtx.Headers.Add(HeaderName, HeaderValues);

            return true;
        }





    }
}
