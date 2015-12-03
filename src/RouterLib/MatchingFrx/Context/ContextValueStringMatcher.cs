using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// Matches an entry in the context (based on string comparison)
    /// </summary>
    public class ContextValueStringMatcher : ContextContainsKeyMatcher
    {
        
        public string ExpectedValue { get; set; }
        public StringMatchType ValueMatchType { get; set; } = StringMatchType.Exact;


        public ContextValueStringMatcher()
        {

        }
        public ContextValueStringMatcher(string keyName, string value) : this(keyName, value, StringMatchType.Exact)
        {


        }

        public ContextValueStringMatcher(string keyName, string expectedValue, StringMatchType matchType ) : base(keyName, StringMatchType.Exact)
        {

        

            KeyName = keyName;
            ExpectedValue = expectedValue;
            ValueMatchType = matchType;

        }


        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                             string sAddress,
                                             IDictionary<string, object> Context,
                                             Stream Body)
        {

            

            if (string.IsNullOrEmpty(ExpectedValue))
                throw new ArgumentNullException("ExpectedValue");



            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;


            return MatchString(Context[KeyName].ToString(), ExpectedValue, ValueMatchType);
        }

    }
}
