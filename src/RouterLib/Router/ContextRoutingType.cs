using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public enum ContextRoutingType
    {
        /// <summary>
        /// Address list contain 1 or many
        /// the first address will be used
        /// </summary>
        Single, 
        /// <summary>
        /// The call is broadcasted to all hosts in the address list, 
        /// the resturn will be the fastest responder
        /// </summary>
        FastestRoute,
        /// <summary>
        /// Call is broadcasted to all hosts in the address list, 
        /// response is aggregated (array of Result).
        /// </summary>
        ScatterGather,
        /// <summary>
        /// call is load balanced using R/R 
        /// </summary>
        RoundRobin
    }
    

}
