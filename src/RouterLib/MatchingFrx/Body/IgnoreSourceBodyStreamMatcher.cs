using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    /// <summary>
    /// will always return true
    /// </summary>
    public class IgnoreSourceBodyStreamMatcher : ReplaceBodyStreamMatcher
    {
        public IgnoreSourceBodyStreamMatcher() : base(null)
        {
            // no op

        }
    }
}
