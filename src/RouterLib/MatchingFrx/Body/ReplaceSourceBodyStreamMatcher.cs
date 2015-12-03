using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// will always return true
    /// </summary>
    public class ReplaceBodyStreamMatcher : MatcherBase
    {
        public Stream NewBody { get; set;}
        public ReplaceBodyStreamMatcher(Stream newBody) 
        {
            NewBody = newBody;

        }


        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {



            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            routingContext.SourceBody = NewBody;
            return true;

        }

    }
}
