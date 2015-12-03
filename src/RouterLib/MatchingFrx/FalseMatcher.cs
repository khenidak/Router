using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// will always return false
    /// </summary>
    public class FalseMatcher : StaticMatcher
    {
    
        public FalseMatcher() : base(false)
        {

        }
        


    }
}
