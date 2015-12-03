using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class ThrowIfInnerExceptionNotIn : ContextExecuteStrategyBase
    {
        public List<Exception> Exceptions { get; set; } 
        public ThrowIfInnerExceptionNotIn(params Exception[] exceptions)
        {
            Exceptions = new List<Exception>(exceptions);
        }

        public override  Task<ContextExecuteModeBase> ExecuteStrategyAsync(int CallCount, RoutingContextBase re, AggregateException ae)
        {
            var exps = (null == Exceptions) ? new List<Exception>() : Exceptions;

            if (0 == exps.Count())
                return Task.FromResult((ContextExecuteModeBase) new DoNotRetryMode());


            var aex = ae.Flatten();

            if (exps.Contains(aex.InnerException))
                return Task.FromResult((ContextExecuteModeBase)new RetryMode());

            return Task.FromResult((ContextExecuteModeBase) new DoNotRetryMode());
        }
    }
    
    public static class ThrowExceptionExt
    {
        public static ContextExecuteStrategyBase ThrowIfExceptionIsIn(this ContextExecuteStrategyBase strategy, params Exception[] exceptions)
        {
            return strategy.Then(new ThrowIfInnerExceptionNotIn(exceptions));
        }
    }
}
