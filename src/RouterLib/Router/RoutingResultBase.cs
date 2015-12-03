using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// represents the Context Execution Results. acts as a container for results. 
    /// this enables repeatable execute on context if needed. 
    /// </summary>
    public abstract class RoutingResultBase 
    {
        private bool mResultSet = false;        
        protected object mRawResult = null;


        public RoutingContextBase ExecutionContext;
        public object RawResult { get { return mRawResult; } }
        public void SetResult(object RawResult)
        {
            if (mResultSet)
                throw new InvalidOperationException("result are already set");

            mRawResult = RawResult;

            mResultSet = true;
        }

        public abstract Task<T> ResultAsAsync<T>();



    }
}
