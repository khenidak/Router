using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// inline if like
    /// IIF(condition predicate, true predicate, false predicate)
    ///</summary>
    public class IIFMatcher : MatcherBase
    {
        public MatcherBase Predicate { get; set; }
        public MatcherBase True{ get; set; }
        public MatcherBase False { get; set; }

        public IIFMatcher()
        {

        }
        public IIFMatcher(MatcherBase predicate, MatcherBase truePredicate, MatcherBase falsePredicate) 
        {
            Predicate = predicate;
            True = truePredicate;
            False = falsePredicate;
        }

        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {


            if (null == Predicate)
                throw new ArgumentNullException("Predicate");




            if (false == await base.MatchAsync(routingContext, sAddress, Context, Body))
                return false;



            if (true == await Predicate.MatchAsync(routingContext, sAddress, Context, Body))
            {
                if (null != True)
                    return await True.MatchAsync(routingContext, sAddress, Context, Body);

            }
            else
            {
                if (null != False)
                    return await False.MatchAsync(routingContext, sAddress, Context, Body);

            }


            return true;
        }


    }
}
