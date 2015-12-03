using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using RouterLib;


namespace HttpRouterLib
{
    /// <summary>
    /// This stratgy overrides the host address list with a specific host address (provided)
    /// typically used to route failed requests to a different endpoint.
    /// </summary>
    public class RouteToHostStrategy : ContextExecuteStrategyBase
    {
        public string TargetHostAddress { get; set; }
        public RouteToHostStrategy() 
        {

        }

        public RouteToHostStrategy(string targetHostAddress)
        {
            TargetHostAddress = targetHostAddress;
        }

        public override Task<ContextExecuteModeBase> ExecuteStrategyAsync(int CallCount, RoutingContextBase re, AggregateException ae)
        {
            if (string.IsNullOrEmpty(TargetHostAddress))
                throw new InvalidOperationException("Target host is null or empty");

            // apply strategy
            var httpCtx = re as HttpRoutingContext;

            if(null == httpCtx)
                throw new InvalidOperationException("Route to host only supports Http Routing Contexts");

            // clear all address
            httpCtx.TargetHostAddressList.Clear();

            // route to my host
            httpCtx.TargetHostAddressList.Add(TargetHostAddress); 
            httpCtx.RouteExecuteType = ContextRoutingType.Single;



            return Task.FromResult((ContextExecuteModeBase) new RetryMode());
        }
    }

    public static class RouteToHostExt
    {
        public static ContextExecuteStrategyBase ThenRouteToHost(this ContextExecuteStrategyBase current, string targetHostAddress)
        {
            current.Next = new RouteToHostStrategy(targetHostAddress);
            return current.Next;

        }
    }
}
