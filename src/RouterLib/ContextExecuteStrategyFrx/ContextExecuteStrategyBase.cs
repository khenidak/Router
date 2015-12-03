using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    
   /// <summary>
   /// Used by the router to handle errors upon context execution. The strategy is represented as a simple linked list.
   /// The Router will attempt use the head passing in callcounter, routingcontext,error. if the return mod is marked as ShouldRouterRetry
   /// it will retry the Context Execution call passing in the mode, then moves to the next strategy in the list.
   /// </summary>
    public abstract class ContextExecuteStrategyBase 
    {
        public ContextExecuteStrategyBase Next { get; set; } 


        public abstract Task<ContextExecuteModeBase> ExecuteStrategyAsync(int CallCount, RoutingContextBase re ,AggregateException ae);


        public virtual ContextExecuteStrategyBase Then(ContextExecuteStrategyBase next)
        {
            this.Next = next;
            return this.Next;
        }
    }
}
