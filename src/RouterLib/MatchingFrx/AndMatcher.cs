using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// will return true if both right and left predicates returned true
    /// </summary>
    public class AndMatcher : OperandMatcherBase
    {
        public AndMatcher(MatcherBase left, MatcherBase right) : base(left, right)
        {
        }

        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {
            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            var LeftResults = null == Left ? true : await Left.MatchAsync(routingContext, sAddress, Context, Body);

            // short circut
            if (!LeftResults)
                return false;

            var rightResults = null == Right ? true : await Right.MatchAsync(routingContext, sAddress, Context, Body);

            // short circut
            if (!rightResults)
                return false;


            return true;
        }


    }
}
