using RouterLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpRouterLib
{
    public class HttpRoutingResult : RoutingResultBase
    {
   

        public override Task<T> ResultAsAsync<T>()
        {
            return Task.FromResult( (T) mRawResult ) ;

        }
    }
}
