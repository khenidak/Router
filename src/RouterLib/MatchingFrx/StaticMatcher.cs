using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// base class for true or false.
    /// </summary>

    public abstract class StaticMatcher : MatcherBase
    {
        protected bool mReturnValue = false;

        public StaticMatcher(bool returnValue = false)
        {
            mReturnValue = returnValue;
        }


        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {
            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;


            return mReturnValue;
        }


    }
}
