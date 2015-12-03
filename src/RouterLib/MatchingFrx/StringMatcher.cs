using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RouterLib
{
    public enum StringMatchType
    {
        Exact,
        UriHostandPortMatch,
        UriHostNameMatch,
        UriHostPortMatch,
        StartsWith,
        Contains,
        RegExpression
    }

    /// <summary>
    /// basic string matcher that matches performes comparison, uri based comparison, or regex on 2 strings
    /// </summary>
    public class StringMatcher :MatcherBase
    {
        public string Match{get; set;}
        public string MatchTo { get; set; }
        public StringMatchType MatchType = StringMatchType.Exact;

        public StringMatcher()
        {

        }

        public StringMatcher(string match, string matchTo, StringMatchType matchType)
        {
            Match = match;
            MatchTo = matchTo;
            MatchType = matchType;
        }


        public async override Task<bool> MatchAsync(RoutingContextBase routingContext,
                                                    string sAddress,
                                                    IDictionary<string, object> Context,
                                                    Stream Body)
        {
            // do not validated against empty string here. 

            if (null == Match)
                throw new ArgumentNullException("string match (left)");


            if (null == MatchTo)
                throw new ArgumentNullException("string match to(right)");


            if (false == await base.MatchAsync( routingContext, sAddress, Context, Body))
                return false;


            return MatchString(Match, MatchTo, MatchType);



        }

    }
}
