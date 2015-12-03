using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{

    /// <summary>
    /// Represents an execution context created as the result of executing a resolve tree
    /// at the basic funciton it maintenane
    /// List of host address, which the call should go to
    /// Routing type (on host addresses) single, fastest, loadbalanced 
    /// 
    /// as the router executes the strategy (for error handling, retries etc) it will pass
    /// a mode to ExecuteAsync, this mode can be anything. hence each context defines the allowed 
    /// modes (for example a context may choose not to honor Route to a different host)
    /// 
    /// at minimum context must honor retry, don not retry modes. 
    /// </summary>

    public abstract class RoutingContextBase 
    {

        private List<Type> mDefaultAllowedExecuteModesTypes =
            new List<Type>() { typeof(DoNotRetryMode), typeof(RetryMode) };



        protected IRouteResolver mResolver;


        protected List<string> mTargetAddresses = new List<string>();


        public string SourceAddress { get; set;}
        public IDictionary<string, object> SourceContext { get; set; }
        public Stream SourceBody { get; set;}


        public ContextRoutingType RouteExecuteType { get; set; } = ContextRoutingType.Single;
        public List<string> TargetHostAddressList { get { return mTargetAddresses; } }
        public Dictionary<string, object> Context { get; set; }
        public string MatcherTreeId { get; set; } // provdided by the resolve framework, allows the context to load any cached data.

        protected Stream getBodyCopy()
        {
            if (null == SourceBody)
                return null;

            MemoryStream ms = new MemoryStream();
            SourceBody.CopyTo(ms);
            ms.Seek(0, 0);
            return ms;
        }

        protected void AddAllowedExecuteMode(params Type[] types)
        {
            if (null == types)
                return;

            foreach (var t in types)
            {
                if (t != null && t != typeof(DoNotRetryMode) && t != typeof(RetryMode))
                    mDefaultAllowedExecuteModesTypes.Add(t);
            }
        }


        protected void RemoveAllowedExecuteMode(params Type[] types)
        {
            if (null == types)
                return;

            foreach (var t in types)
            {
                if (t != null && t != typeof(DoNotRetryMode) && t != typeof(RetryMode))
                    mDefaultAllowedExecuteModesTypes.Remove(t);
            }
        }
        protected bool IsAllowedExecutionMode(ContextExecuteModeBase executeMode)
        {
            if (null == executeMode)
                throw new ArgumentNullException("executeMode");

            return mDefaultAllowedExecuteModesTypes.Contains(executeMode.GetType());
        } 

        public RoutingContextBase(IRouteResolver Resolver,
                                  string sAddress, 
                                  IDictionary<string, object> Context,
                                  Stream Body)
        {
            // Copy variables locally for this execution context
            SourceAddress = sAddress;
            SourceContext = Context;
            SourceBody = Body;
            mResolver = Resolver;
        }


        public abstract Task<RoutingResultBase> ExecuteAsync(ContextExecuteModeBase executeMode);
    }
}
