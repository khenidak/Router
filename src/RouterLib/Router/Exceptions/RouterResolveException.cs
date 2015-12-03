using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class RouterResolveException : AggregateException
    {
        public RouterResolveException(params Exception[] innerExceptions) : base(innerExceptions)
        {
        }
    }
}
