using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// The Router class ties every thing together. it creates a logical instance context per call (in the context dictionary described by id) it:
    /// 1- Uses a resolver to perform resolve on the incoming Routing requests (resolve creates an execution context). 
    /// 2- Executes the context and perform the context execute strategy (retry, retry with backoffs etc). 
    /// 3- Provide basic Execute & Resolve telemetry
    /// 
    /// You don't need to override any of the functionlity below unless you have to
    /// for example HttpRouter overrides Ctor to demand a resolver
    /// </summary>
    /// <typeparam name="RER">Type of Results </typeparam>
    /// <typeparam name="R">Type of the Resolver</typeparam>
    public abstract class Router<RER,R> where RER : RoutingResultBase where R : IRouteResolver
    {
        private Clicker<RouterClickBase, double> mMinClicker =
            new Clicker<RouterClickBase, double>(TimeSpan.FromDays(1));

        private Clicker<RouterClickBase, double> mHourClicker =
            new Clicker<RouterClickBase, double>(TimeSpan.FromHours(1));


        protected int mMaxRetryCount = 20; // acts as circurt breaker to override long (aka stupid) execution strategies 

        protected ContextExecuteStrategyBase mDefaultContextExecuteStrategy = null;
        protected R mResolver;

        protected Clicker<RouterClickBase, double> MinClicker { get { return mMinClicker; } }
        protected Clicker<RouterClickBase, double> HourClicker { get { return mHourClicker; } }




        public const string Context_Router_Instance_Key = "Router.Core.Instance";
        public const string Context_Router_InstanceId_Key = "Router.Core.InstanceId";
        public const string Context_Router_InstanceTrace_Key = "Router.Core.InstanceTrace";


        #region ctor
        public Router()
        {

            // clickers trimming
            mMinClicker.OnTrim = (head) => OnMinuteClickerTrim(head);
            mHourClicker.OnTrim = (head) => OnHourClickerTrim(head);
        }
        #endregion

        #region Telemetry Book Keeping

        protected virtual void OnMinuteClickerTrim(RouterClickBase head)
        {
            // roll up totals into hours  
            int totalExecuted = 0;
            int totalResolved = 0;

            double totalExecutionTime = 0;
            double totalResolveTime = 0;

            double averageExecutionTime = 0;
            double averageResolveTime = 0;


            RouterClickBase curr = head;
            while (curr != null)
            {
                switch (curr.ClickType)
                {
                    case RouterClickBase.Execute_ClickType:
                        {
                            totalExecutionTime += curr.Value;
                            ++totalExecuted;
                            break;
                        }
                    case RouterClickBase.Resolve_ClickType:
                        {
                            totalResolveTime += curr.Value;
                            ++totalResolved;
                            break;
                        }
                }

                curr = (RouterClickBase) curr.Next;
            }

            // Collect and rollup

            averageExecutionTime = totalExecuted == 0 ? 0 : totalExecutionTime / totalExecuted;
            averageResolveTime = totalResolved == 0 ? 0 : totalResolveTime / totalResolved;


            if (0 != totalExecuted)
                mHourClicker.Click(new RouterClickBase() { ClickType = RouterClickBase.TotalExecuted_ClickType, Value = totalExecuted });

            if (0 != totalResolved)
                mHourClicker.Click(new RouterClickBase() { ClickType = RouterClickBase.TotalResolved_ClickType, Value = totalResolved });


            if (0 != averageExecutionTime)
                mHourClicker.Click(new RouterClickBase() { ClickType = RouterClickBase.AvgExecuteTime_ClickType, Value = averageExecutionTime });

            if (0 != averageResolveTime)
                mHourClicker.Click(new RouterClickBase() { ClickType = RouterClickBase.AvgResolveTime_ClickType, Value = averageResolveTime });
        }

        protected virtual void OnHourClickerTrim(RouterClickBase head)
        {
            // no op, in your sub class you may choose to presist them some where
        }

        protected virtual void OnTelemetryExecute(RoutingContextBase routingContext, 
                                                  RoutingResultBase routingResults, 
                                                  double totalms)
        {
            mMinClicker.Click(new RouterClickBase() { ClickType = RouterClickBase.Execute_ClickType, Value = totalms });

        }

        protected virtual void OnTelemetryResolve(RoutingContextBase routingContext, 
                                                  double totalms)
        {
            mMinClicker.Click(new RouterClickBase() { ClickType = RouterClickBase.Resolve_ClickType, Value = totalms });
        }

        #endregion

        #region Telemetry Specs
        public int TotalExecutedLastMin
        {
            get
            {
                return mMinClicker.Do(
                    head =>
                    {
                        int count = 0;
                        RouterClickBase curr = head;
                        while (curr != null)
                        {
                            if (curr.ClickType == RouterClickBase.Execute_ClickType)
                            {
                                count++;
                            }

                            curr = (RouterClickBase)curr.Next;
                        }
                        return count;
                    });
            }
        }
        public int TotalResolvedLastMin
        {
            get
            {
                return mMinClicker.Do(
                    head =>
                    {
                        int count = 0;
                        RouterClickBase curr = head;
                        while (curr != null)
                        {
                            if (curr.ClickType == RouterClickBase.Resolve_ClickType)
                            {
                                count++;
                            }

                            curr = (RouterClickBase)curr.Next;
                        }
                        return count;
                    });
            }
        }
        public int TotalExecutedLastHour
        {
            get
            {
                return mHourClicker.Do(
                    head =>
                    {
                        int count = 0;
                        RouterClickBase curr = head;
                        while (curr != null)
                        {
                            if (curr.ClickType == RouterClickBase.TotalExecuted_ClickType)
                            {
                                count += (int) curr.Value ;
                            }

                            curr = (RouterClickBase)curr.Next;
                        }
                        return count;
                    });
            }
        }
        public int TotalResolvedLastHour
        {
            get
            {
                return mHourClicker.Do(
                    head =>
                    {
                        int count = 0;
                        RouterClickBase curr = head;
                        while (curr != null)
                        {
                            if (curr.ClickType == RouterClickBase.TotalResolved_ClickType)
                            {
                                count += (int) curr.Value ;
                            }

                            curr = (RouterClickBase)curr.Next;
                        }
                        return count;
                    });
            }
        }
        public double AvgResolveTimePerMinLastHour
        {
            get
            {
                return mHourClicker.Do(
                    head =>
                    {
                        int count = 0;
                        double totalavgs = 0;
                        RouterClickBase curr = head;
                        while (curr != null)
                        {
                            if (curr.ClickType == RouterClickBase.AvgResolveTime_ClickType)
                            {
                                count++;
                                totalavgs += curr.Value;

                            }

                            curr = (RouterClickBase)curr.Next;
                        }
                        return 0 == count ? 0 : totalavgs / count;
                    });
            }
        }
        public double AvgExecuteTimePerMinLastHour
        {
            get
            {
                return mHourClicker.Do(
                    head =>
                    {
                        int count = 0;
                        double totalavgs = 0;
                        RouterClickBase curr = head;
                        while (curr != null)
                        {
                            if (curr.ClickType == RouterClickBase.AvgExecuteTime_ClickType)
                            {
                                count++;
                                totalavgs += curr.Value;
                            }

                            curr = (RouterClickBase)curr.Next;
                        }
                        return 0 == count ? 0 : totalavgs / count;
                    });
            }
        }
#endregion

        public ContextExecuteStrategyBase DefaultContextExecuteStrategy
        {
            get
            {
                return this.mDefaultContextExecuteStrategy;   
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException();

                mDefaultContextExecuteStrategy = value;
            }
        }
        
        #region RouteAsync et all
        /// <summary>
        /// Routes the call using the ResolverFrx
        /// </summary>
        /// <param name="sAddress">Address which the call was sent to</param>
        /// <returns>Routing Result</returns>
        public Task<RER> RouteAsync(string sAddress)
        {
            return RouteAsync(sAddress, null, null, null);
        }

        /// <summary>
        /// Routes the call using the ResolverFrx
        /// </summary>
        /// <param name="sAddress">Address which the call was sent to</param>
        /// <param name="Body">Message Body/Call Body</param>
        /// <returns>Routing Result</returns>
        public Task<RER> RouteAsync(string sAddress, Stream Body)
        {
            return RouteAsync(sAddress, null, Body, null);
        }
        /// <summary>
        /// Routes the call using the ResolverFrx
        /// </summary>
        /// <param name="sAddress">Address which the call was sent to</param>
        /// <param name="Context">Dictionary of context variables used for routing and/or processing</param>
        /// <returns>Routing Result</returns>
        public Task<RER> RouteAsync(string sAddress, IDictionary<string, object> Context)
        {
            return RouteAsync(sAddress, Context, null, null);

        }

        /// <summary>
        /// Routes the call using the ResolverFrx
        /// </summary>
        /// <param name="sAddress">Address which the call was sent to</param>
        /// <param name="Context">Dictionary of context variables used for routing and/or processing</param>
        /// <param name="Body">Message Body/Call Body</param>
        /// <returns>Routing Result</returns>
        public Task<RER> RouteAsync(string sAddress, IDictionary<string, object> Context, Stream Body)
        {
            return RouteAsync(sAddress, Context, Body, null);
        }


        protected virtual async Task<RoutingContextBase> ResolveAsync(string sAddress, IDictionary<string, object> Context, Stream Body)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start(); // start the clock
                var re = await mResolver.ResolveAsync(sAddress, Context, Body);
            sw.Stop(); // stop

            // add telemetry;
            OnTelemetryResolve(re, sw.Elapsed.TotalMilliseconds);
            
            
            // return;
            return re;
        }


        protected virtual async Task<RER> ExecuteContextAsync(RoutingContextBase re, ContextExecuteStrategyBase chs)
        {
            RER rer = null;
            Stopwatch sw = new Stopwatch();
            var currentChs = chs;
            ContextExecuteModeBase ExecuteMode = new DoNotRetryMode();
            var TryCount = 1;
            var bSucess = false;
            
            
            // todo: execute call handling logic

            while (true)
            {
                try
                {
                    sw.Start();
                    rer = (RER)await re.ExecuteAsync(ExecuteMode);
                    sw.Stop();
                    bSucess = true;
                }
                catch (Exception e)
                {
                    if (null == currentChs)
                        throw;

                    TryCount++;

                    // circut breaker
                    if (TryCount > mMaxRetryCount)
                        throw new RouterMaxedRetryException(new AggregateException(e)) { RetryCount = (TryCount - 1), RoutingContext =re };


                    try
                    {
                        // execute current strategy;
                        ExecuteMode = await currentChs.ExecuteStrategyAsync(TryCount, re, new AggregateException(e));
                    }
                    catch (Exception StrategyExecuteFail)
                    {
                        // if we failed to execute strategy we just wrap the exception
                        // along with the orginal execution one.
                        throw new FailedToExecuteStrategyException(StrategyExecuteFail, e) { TryCount = TryCount, RoutingContext = re};
                    }

                    // move to next strategy
                    currentChs = currentChs.Next;


                    if (!ExecuteMode.ShouldRouterTryAgain)
                        throw new AggregateException(e);

                }
                
                finally
                {
                    OnTelemetryExecute(re, rer, sw.Elapsed.TotalMilliseconds);
                    sw.Reset(); // reset the clock
                }

                if (bSucess)
                    break; // exit the loop    
            }


            return rer;


        }


        /// <summary>
        /// Routes the call using the ResolverFrx
        /// </summary>
        /// <param name="sAddress">Address which the call was sent to</param>
        /// <param name="Context">Dictionary of context variables used for routing and/or processing</param>
        /// <param name="Body">Message Body/Call Body</param>
        /// <param name="overrideDefaultExecutionStrategy">overrides the router's default execution strategy</param>
        /// <returns>Routing Result</returns>
        public virtual async Task<RER> RouteAsync(string sAddress,
                                     IDictionary<string, object> Context, 
                                     Stream Body,
                                     ContextExecuteStrategyBase overrideDefaultExecutionStrategy)
        {
            // context management
            var chs = overrideDefaultExecutionStrategy == null ? mDefaultContextExecuteStrategy : overrideDefaultExecutionStrategy;
            RoutingContextBase re;
            RER rer;

            Context = null != Context ? Context : new Dictionary<string, object>() ;
            // can be used by various sub system involved to reach the router, unique call instance id, or the trace log
            Context.Add(Context_Router_Instance_Key, this); // router 
            Context.Add(Context_Router_InstanceId_Key, Guid.NewGuid().ToString()); // instance id
            Context.Add(Context_Router_InstanceTrace_Key, string.Empty); // trace 


            // resolve             
            try
            {
                re = await ResolveAsync(sAddress, Context, Body);
            }

            // caller can handle this ae, or RouterResolveException
            catch (AggregateException ae)
            {
                throw new RouterResolveException(ae);
            }
            catch(Exception e)
            {
                // this is either a telemetry error, and shouldn't happen in runtime 
                // (unless you are doing something funny with the telemetry sub system)
                // or you have a sub class that is doing something funny.
                throw new RouterResolveException(e);
            }


            // if no resolver is matching then we need return null
            if (null == re)
                return null;

            // next, we execute
            try
            {
                rer = await ExecuteContextAsync(re, chs);
            }
            catch
            {
                throw;
            }
            finally
            {
                // context maintenance
                // just in case the caller intersted in reusing the dictionary
                Context.Remove(Context_Router_Instance_Key);
                Context.Remove(Context_Router_InstanceId_Key);
                Context.Remove(Context_Router_InstanceTrace_Key);
            }
            return rer;
       }
        #endregion  
    }
}
