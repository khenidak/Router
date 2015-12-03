
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// Matching framework is used by ResolverFx. Each matcher represents a tree (linked list in actualty)
    /// Resolver calls Head will do Next.MatchAsync->Next.MatchAsync->Next.MatchAsync..
    /// inside each matcher the logic is as the following:
    ///     ->If next returned true, execute the local matchng logic
    ///     -> if not return and DON'T execute the logic
    /// 
    /// Why is it refered to as Tree? in the Lib you have 
    /// And, Or, IIF, Any, All Matchers. each allows the logic to branch as needed.
    /// 
    /// Each matcher gets a reference to the ExecutionContext in which it can modify as needed. 
    /// Keep in mind address, body and context are assigned to the Context by the resolver.     
    /// </summary>

    public abstract class MatcherBase
    {

        /* 
            this id is copied to RoutingContextBase.
            the idea is, if you need to cache info based on matching tree results
            then you can use this tree id as key. for example a round robin load balancing set
            will need mark last host used so it uses the next. another example can cached routing results
        
            this ensure that a load balancing set saved differently even if the same different balancing
            set appears in multiple trees. 

            this id is kept unique for every tree. not a unique name per matcher 
        */
        public string MatcherTreeId { get; set; } = Guid.NewGuid().ToString();
        public MatcherBase Next { get; set; }

        

        public async virtual Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                   string sAddress,
                                                   IDictionary<string, object> Context,
                                                   Stream Body)
        {

            if (null == Next)
            {
                // only the last (linked list tail) matcher sets the id.
                // copy the matcherTreeId to context;
                routingContext.MatcherTreeId = this.MatcherTreeId;
            }
            else
            {
                if (null != Next)
                    return await Next.MatchAsync(routingContext, sAddress, Context, Body);

            }

            return true;
        }

        #region Helper Funcs for all matchers
        public void Chain( params MatcherBase[] chain)
        {
            if (null == chain)
                throw new ArgumentNullException("Chain");

            var anyNull = chain.Where(m => null == m);

            if (0 != anyNull.Count())
                throw new ArgumentNullException("Chain contains one more more null entries");


            var current = this;
            foreach (var matcher in chain)
            {
                current.Next = matcher;
                current = current.Next;
            }
        }

        protected bool MatchString(string sLeft, string sRight, StringMatchType matchType = StringMatchType.Exact)
        {

            var Left = sLeft.ToLowerInvariant();
            var Right = sRight.ToLowerInvariant();
            // dump matching logic 
            switch (matchType)
            {
                case StringMatchType.Exact:
                    return (Left == Right);

                case StringMatchType.Contains:
                    return (Left.Contains(Right));

                case StringMatchType.StartsWith:
                    return (Left.StartsWith(Right));

                case StringMatchType.UriHostandPortMatch:
                    {
                        var LeftUri = new Uri(Left);
                        if (sRight.Contains(":"))
                            return (string.Concat(LeftUri.Host, ":", LeftUri.Port) == Right);
                        else
                            return (LeftUri.Host == Right);
                    }

                case StringMatchType.UriHostNameMatch:
                    {
                        var LeftUri = new Uri(Left);
                        return (LeftUri.Host == Right);
                    }

                case StringMatchType.UriHostPortMatch:
                    {
                        var LeftUri = new Uri(Left);
                        return (LeftUri.Port.ToString() == Right);
                    }


                // for RegEx we don't lower the variable
                case StringMatchType.RegExpression:
                    return (new Regex(sRight)).Match(sLeft).Success;

                default:
                    throw new InvalidOperationException(string.Format("string matcher encountered unknown string match type:{0}", matchType.ToString()));
            }
        }
        
        
         
        
        #endregion

    }
}
