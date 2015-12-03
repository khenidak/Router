using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    ///  base class for And/Or
    /// </summary>
    public abstract class OperandMatcherBase : MatcherBase
    {

        public MatcherBase Left;
        public MatcherBase Right;

        public OperandMatcherBase(MatcherBase left, MatcherBase right)
        {
            Left = left;
            Right = right;
        }
        


    }
}
