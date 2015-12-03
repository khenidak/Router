using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// will return true if any of the predicates returned true (and will stop executing the rest of predicates).
    /// </summary>
    public class AnyMatcher : MatcherBase
    {
        protected MatcherBase[] mPredicates = null;
        public AnyMatcher(params MatcherBase[] Predicates) 
        {
            if (null == Predicates)
                throw new ArgumentNullException("Predicates");

            var anyNull = Predicates.Where(p => null == p);

            if (0 != anyNull.Count())
                throw new ArgumentNullException("one or more predicates are null");


            mPredicates = Predicates;

        }

        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {
            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;


            foreach (var p in mPredicates)
            {
                var success = await p.MatchAsync(routingContext, sAddress, Context, Body);
                if (success)
                    return true;

            }
            return false;
        }


    }
}
