using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class ContextContainsKeyMatcher  : MatcherBase
    {
        public string KeyName { get; set; }
        public StringMatchType MatchType { get; set; } = StringMatchType.Exact;


        public ContextContainsKeyMatcher()
        {

        }
        public ContextContainsKeyMatcher(string keyName) : this(keyName, StringMatchType.Exact)
        {
            
        }

        public ContextContainsKeyMatcher(string keyName, StringMatchType matchType )
        {
            KeyName = keyName;
            MatchType = matchType;
        }


        
        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                             string sAddress,
                                             IDictionary<string, object> Context,
                                             Stream Body)
        {
            if (string.IsNullOrEmpty(KeyName))
                throw new ArgumentNullException("KeyName");


            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;



            // short circut, if comparison is exact check that the dictionary 
            // contains exact key &  we are done.
            if (MatchType == StringMatchType.Exact)
                return Context.ContainsKey(KeyName);
            

            

            foreach (var key in Context.Keys)
            {

                if (MatchString(key,KeyName, MatchType))
                    return true;
            }

            return false;


        }
    }
}
