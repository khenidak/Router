using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// if you have a very simple resolve mechasim then you can create your own 
    /// resolver implementing this interface. this by passes the usage of Matching trees all together
    /// </summary>
    public interface IRouteResolver 
    {

        ResolverState State { get; }
        Task<RoutingContextBase> ResolveAsync(string sAddress,
                                        IDictionary<string, object> Context,
                                        Stream Body);


        Task AddMatcherAsync(MatcherBase matcher, int Order);
        Task RemoveMatcherAsync(int Order);
    }
}
