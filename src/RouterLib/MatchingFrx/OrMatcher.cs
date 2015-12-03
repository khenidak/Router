using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// Will return true if left or right are true.
    /// </summary>
    public class OrMatcher : OperandMatcherBase
    {
        public OrMatcher(MatcherBase left, MatcherBase right) : base(left, right)
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
            var rightResults = null == Right ? true : await Right.MatchAsync(routingContext, sAddress, Context, Body);

            return LeftResults || rightResults;
        }


    }
}
