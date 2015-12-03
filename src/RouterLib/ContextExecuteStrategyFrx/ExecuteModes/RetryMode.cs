using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    
    /// <summary>
    /// This will make the router retry the context execute call in case of error (as returned from the strategy execution)
    /// This mode is passed to RoutingContextBase.ExecuteAsync. if you need to add more moods sub class as needed. 
    /// </summary>
    public sealed class RetryMode : ContextExecuteModeBase
    {
        public RetryMode() : base(true) { }
    }


}
