using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// will return true if all the predicates returned true.
    /// </summary>
    public class AllMatcher : MatcherBase
    {
        public MatcherBase[] Predicates { get; set; }
        public AllMatcher()
        {

        }
        public AllMatcher(params MatcherBase[] predicates) 
        {
            Predicates = predicates;
        }

        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {


            if (null == Predicates)
                throw new ArgumentNullException("Predicates");

            var anyNull = Predicates.Where(p => null == p);

            if (0 != anyNull.Count())
                throw new ArgumentNullException("one or more predicates ids null");



            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;


            foreach (var p in Predicates)
            {
                var success = await p.MatchAsync(routingContext, sAddress, Context, Body);
                if (!success)
                    return false;

            }
            return true;
        }


    }
}
