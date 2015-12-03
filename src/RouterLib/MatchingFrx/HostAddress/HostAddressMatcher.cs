using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// Matches the host address (the parameter to MatchAsync) based on string comparison
    /// </summary>
    public class HostAddressMatcher: MatcherBase
    {
        public string ExpectedHostAddress { get; set; }
        public StringMatchType HostAddressMatchType { get; set; } = StringMatchType.Exact;

        public HostAddressMatcher(string expectedHostAddress) : this (expectedHostAddress, StringMatchType.Exact)
        {
            // no op
        }

        public HostAddressMatcher(string expectedHostAddress, StringMatchType hostAddressMatchType)
        {
            ExpectedHostAddress = expectedHostAddress;
            HostAddressMatchType = hostAddressMatchType;
        }  
        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                             string sAddress,
                                             IDictionary<string, object> Context,
                                             Stream Body)
        {

            if (string.IsNullOrEmpty(ExpectedHostAddress))
                throw new ArgumentNullException("ExpectedHostAddress");


            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;


            return MatchString(sAddress, ExpectedHostAddress, HostAddressMatchType);

        }
    }
}
