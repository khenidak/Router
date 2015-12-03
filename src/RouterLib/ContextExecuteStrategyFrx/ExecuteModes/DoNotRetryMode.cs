using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    

    public sealed class DoNotRetryMode : ContextExecuteModeBase
    {
        public DoNotRetryMode() : base(false) { }
    }


}
