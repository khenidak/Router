using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// The resolver is the entry point for the resolve Fx. it sets on top of a set matcher trees, each is assigned a priority
    /// when resolving it will try to resolve using the matching trees according to order if one matched, it will be used 
    /// if none matched a null is returned. 
    /// 
    /// Resolver also provide a state for all matcher trees used by it. represented by the mState
    /// matchers can use it from stuff like load balancing (keeping track of last host address used) or general caching
    /// </summary>
    public abstract class ResolverBase : IRouteResolver
    {
        private int Changing = 0;
        private SpinWait locker  = new SpinWait(); 
        private SortedDictionary<int, MatcherBase> mMatchList = new SortedDictionary<int, MatcherBase>();

        protected ResolverState mState = new ResolverState();

        public ResolverState State { get { return mState; } }


        

        public Task AddMatcherAsync(MatcherBase matcher, int Order)
        {
            if (null == matcher)
                throw new ArgumentNullException("matcher");

            return Task.Run(() =>
            {
                while (0 != Interlocked.CompareExchange(ref Changing, 1, 0))
                    locker.SpinOnce();

                mMatchList.Add(Order, matcher);
                Changing = 0;

            });
        }

        public Task RemoveMatcherAsync(int Order)
        {
            return Task.Run(() =>
            {
                while (0 != Interlocked.CompareExchange(ref Changing, 1, 0))
                    locker.SpinOnce();

                mMatchList.Remove(Order);
                    Changing = 0;
            });
        }



        protected virtual async Task<RoutingContextBase> ResolveAsync(RoutingContextBase re,
                                                               string sAddress,
                                                              IDictionary<string, object> Context,
                                                              Stream Body)
        {
            



            bool bMatched = false;

            while (0 != Interlocked.CompareExchange(ref Changing, 1, 0))
                locker.SpinOnce();

            try
            {
                // can not do matching on empty list

                if (0 == mMatchList.Count)
                    throw new InvalidOperationException("Resolver can not work empty matchers!");

                // try to match
                foreach (var matcher in mMatchList.Values)
                {
                    bMatched = await matcher.MatchAsync(re, sAddress, Context, Body);
                    if (bMatched) break;
                }
            }
            finally
            {
                Changing = 0;
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
