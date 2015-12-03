using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public abstract class ContextExecuteModeBase
    {
        public bool ShouldRouterTryAgain { get; set; } = false;

        public ContextExecuteModeBase()
        {

        }

        public ContextExecuteModeBase(bool tryAgain)
        {
            ShouldRouterTryAgain = tryAgain;
        }
    }

}
