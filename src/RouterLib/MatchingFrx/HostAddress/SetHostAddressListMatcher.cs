using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{

    /// <summary>
    /// sets address or address list to specific list +
    /// modifies routing type.
    /// </summary>
    public class SetAddressListMatcher : MatcherBase
    {
        public bool Clear { get; set; } = false;
        public List<string> TargetHostAddressList { get; set; } = new List<string>();
        public ContextRoutingType RoutingType { get; set; } = ContextRoutingType.Single;

        public SetAddressListMatcher(string HostAddress, 
                                     bool bClear = false, 
                                     ContextRoutingType routingType = ContextRoutingType.Single):
            this(new List<string>(new string[] {HostAddress}), bClear, routingType)
        {
        }

        public SetAddressListMatcher( string[] HostAddress, bool bClear = false, 
                                      ContextRoutingType routingType = ContextRoutingType.Single) 
                : this(new List<string>(HostAddress), bClear, routingType)
        {
            
        }

        public SetAddressListMatcher(List<string> targetHostAddresses, 
                                     bool bClear = false, 
                                     ContextRoutingType routingType = ContextRoutingType.Single)
        {
            RoutingType = routingType;
            TargetHostAddressList = targetHostAddresses;
            Clear = bClear;
        }


        public override async Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                string sAddress,
                                                IDictionary<string, object> Context,
                                                Stream Body)
        {

            // validate, null list is bad
            if (null == this.TargetHostAddressList)
                throw new ArgumentNullException("TargetHostAddresses");


            // a list that has a null or empty is also bad
            var nullAddresses = TargetHostAddressList.Where(s => string.IsNullOrEmpty(s));

            if (0 != nullAddresses.Count())
                throw new ArgumentNullException("TargetHostAddresses contains null or empty host addresses");



            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;

            if (Clear)
                routingContext.TargetHostAddressList.Clear();


            routingContext.TargetHostAddressList.AddRange(TargetHostAddressList);
            routingContext.RouteExecuteType = this.RoutingType;


            return true;
        }

    }
}
