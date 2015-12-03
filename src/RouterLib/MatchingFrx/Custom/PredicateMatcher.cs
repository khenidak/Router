using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// General purpose matchers that uses a func delegate.
    /// </summary>
    public class PredicateMatcher : MatcherBase
    {
        public Func<RoutingContextBase,string, IDictionary<string,object>, Stream, bool> Predicate { get; set; }

        public PredicateMatcher()
        {

        }
        public PredicateMatcher(Func<RoutingContextBase, string, IDictionary<string, object>, Stream, bool> predicate) 
        {
            Predicate = predicate;
        }


        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                             string sAddress,
                                             IDictionary<string, object> Context,
                                             Stream Body)
        {
            if (null == Predicate)
                throw new ArgumentNullException("Predicate");



            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;


            
            return Predicate(routingContext, sAddress, Context, Body);
        }

    }
}
