using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class ResolverState
    {
        protected ConcurrentDictionary<string, object> mStateEntries = new ConcurrentDictionary<string, object>();
        

        public ConcurrentDictionary<string, object> StateEntries
        {
            get { return mStateEntries; }
        }
    }
}
