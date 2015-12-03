using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// The resolver is the entry point for the resolve Fx. it sets on top of a set matcher trees, each is assigned a priority
    /// when resolving it will try to resolve using the matching trees according to order if one matched, it will be used 
    /// if none matched a null is returned. 
    /// </summary>
    public abstract class ResolverBase : IRouteResolver
    {
        protected SortedDictionary<int, MatcherBase> mMatchList = new SortedDictionary<int, MatcherBase>();
        protected ResolverState mState = new ResolverState();
        protected object syncLock = new object(); 
        public ResolverState State { get { return mState; } }


        public Task AddMatcherAsync(MatcherBase matcher, int Order)
        {
            if (null == matcher)
                throw new ArgumentNullException("matcher");

            return Task.Run(() =>
            {
                lock(syncLock)
                    mMatchList.Add(Order, matcher);
            });
        }

        public Task RemoveMatcherAsync(int Order)
        {
            return Task.Run(() =>
            {

                lock (syncLock)
                {
                    mMatchList.Remove(Order);
                }

                    
            });
        }


        protected virtual async Task<RoutingContextBase> ResolveAsync(RoutingContextBase re,
                                                               string sAddress,
                                                              IDictionary<string, object> Context,
                                                              Stream Body)
        {
            // can not do matching on empty list
            if (0 == mMatchList.Count)
                throw new InvalidOperationException("Resolver can not work empty matchers!");




            bool bMatched = false;

            foreach (var matcher in mMatchList.Values)
            {
                bMatched = await matcher.MatchAsync(re, sAddress, Context, Body);
                if (bMatched) break;
            }

            if (!bMatched)
                return null;

            return (RoutingContextBase) re;

        }

        public abstract Task<RoutingContextBase> ResolveAsync(string sAddress,
                                                              IDictionary<string, object> Context,
                                                              Stream Body);
    }
}
