using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class RouterMaxedRetryException : AggregateException
    {
        public int RetryCount;
        public RoutingContextBase RoutingContext {get;set;}
        public RouterMaxedRetryException(params Exception[] innerExceptions) : base(innerExceptions)
        {
        }
    }
}
