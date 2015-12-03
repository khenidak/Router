using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class FailedToExecuteStrategyException : AggregateException
    {


        public int TryCount { get; set; }
        public RoutingContextBase RoutingContext { get; set; }
        public FailedToExecuteStrategyException(params Exception[] innerExceptions) : base(innerExceptions)
        {
        }
    }
}
