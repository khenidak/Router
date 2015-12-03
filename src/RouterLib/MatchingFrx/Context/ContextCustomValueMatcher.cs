using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// Uses a delegate to match against a dictionary value in Context
    /// Func<
    /// string: Key
    /// object: Value
    /// return bool the result of matching
    /// >
    /// </summary>
    public class ContextCustomValueMatcher : ContextContainsKeyMatcher
    {
        public Func<string, object, bool> Predicate { get; set; }

        public ContextCustomValueMatcher()
        {

        }
        public ContextCustomValueMatcher(string keyName, Func<string, object, bool> predicate) : base(keyName, StringMatchType.Exact)
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

            return Predicate(KeyName, Context[KeyName]);
        }

    }
}
