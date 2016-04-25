﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouterLib
{

    /// <summary>
    /// Represents an execution context created as the result of executing a resolve tree
    /// at the basic funciton it maintenane
    /// 1- List of host address, which the call should go to
    /// 2- Routing type (on host addresses) single, fastest, loadbalanced 
    /// 3- Basic R/R load balancing between host addresss
    /// Note on Execute Strategy:
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


        protected const string State_Key_LoadBalancingSet = "http.balancingset-{0}";
        



        protected List<string> mTargetAddresses = new List<string>();


        public IRouteResolver Resolver { get; private set; }

        public string SourceAddress { get; set;}
        public IDictionary<string, object> SourceContext { get; set; }
        public Stream SourceBody { get; set;}


        public ContextRoutingType RouteExecuteType { get; set; } = ContextRoutingType.Single;
        public List<string> TargetHostAddressList { get { return mTargetAddresses; } }
        public string MatcherTreeId { get; set; } // provdided by the matching framework (matcher base class), allows the context to load any cached data.


        public bool OverridePath { get; set; } = false; // if true the path in the source address will
                                                        // be ignored favoring Path property
        public string Path { get; set; } = string.Empty;
        public bool OverrideScheme { get; set; } = false;
        public string Scheme { get; set; } = string.Empty;

        protected virtual string GetNextHostAddressInBalancingSet()
        {
            /*
            Use the the address list as circular buffer with a pointer maintenaned 
            as a state. if the list is empty (or was emptied by mistake) return an exception. 

            we cheated a bit here routing is 
            A-B-C 
            A-B-C
            until we hit int.MaxValue then
            C-B-A
            C-B-A 
            until we hit 0 then
            A-B-C
            A-B-C 

            Overall it is R/R 
            */            

            var sStateKey = string.Format(State_Key_LoadBalancingSet, MatcherTreeId);
            var loadbalanceIdx = (LoadBalanceIdx) Resolver.State.StateEntries.GetOrAdd(sStateKey, (dictKey) => { return new LoadBalanceIdx(); });
            string nextAddress = null;

                while (true)
                {
                    try
                    {
                    
                        var newIdx = Math.Abs(Interlocked.Increment(ref loadbalanceIdx.Idx));

                        // if the list was emptied by mistake, we will go to DivideByZeroException
                        nextAddress = TargetHostAddressList[newIdx % TargetHostAddressList.Count];
                    
                        // so if address was removed after we picked it up, we just try agian
                        if (TargetHostAddressList.Contains(nextAddress))
                                break;
                    }
                    catch (DivideByZeroException)
                    {
                        //somebody cleared the address list while we are trying to pick an address
                        throw new InvalidOperationException("Load balancing set is empty");
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // this will happen if the list got replaced with a shorter list
                        // after we picked a position > sizeof(new list). ingore and try again.
                    }
                    // anyother exception should be vented up the stack.                    
                }
            

            return nextAddress;
        }

      

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
            this.Resolver = Resolver;
        }


        public abstract Task<RoutingResultBase> ExecuteAsync(ContextExecuteModeBase executeMode);
    }
}
