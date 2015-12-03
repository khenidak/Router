using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// Simple descritpive retry logic that the router execute to handle errors
    /// </summary>
    public class RetryAfterStrategy : ContextExecuteStrategyBase
    {
        protected int mDelayMs = 0;

        public RetryAfterStrategy(int delayMs)
        {
            if (delayMs < 0)
                throw new ArgumentOutOfRangeException("retry should be >=0 ");


            mDelayMs = delayMs;
        }

        public override async Task<ContextExecuteModeBase> ExecuteStrategyAsync(int CallCount, RoutingContextBase re, AggregateException ae)
        {
            await Task.Delay(mDelayMs);

            return new RetryMode();
        }
    }

    

    /// <summary>
    /// provides nice coding experience in addition to "retry with Back Off" strategy.
    /// </summary>
    public static class RetryStrategyExt
    {
        public static ContextExecuteStrategyBase ThenTryAgain(this ContextExecuteStrategyBase strategy, 
                                                        int times, 
                                                        int DelayMs)
        {
            var current = strategy;
            for (var i = 1; i <= times; i++)
            {
                current.Next = new RetryAfterStrategy(DelayMs);
                current = current.Next;
            }

            return current;
        }

        /// <summary>
        /// chains multiple retries with different backoffs
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="times"></param>
        /// <param name="DelayMs"></param>
        /// <param name="delayFactor"></param>
        /// <returns></returns>
        public static ContextExecuteStrategyBase ThenTryAgainWithBackOff(this ContextExecuteStrategyBase strategy, 
                                                                   int times, 
                                                                   int DelayMs,
                                                                   float delayFactor)
        {
            var current = strategy;
            for (var i = 1; i <= times; i++)
            {
                var actualDelay =  1 == i ? DelayMs : i * delayFactor * DelayMs;
                
                current.Next = new RetryAfterStrategy((int) actualDelay);
                current = current.Next;
            }

            return current;
        }
    }
}
