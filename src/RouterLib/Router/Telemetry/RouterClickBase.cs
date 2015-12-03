using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public class RouterClickBase : IClick<double>, ICloneable
    {
        public const string Resolve_ClickType = "r";
        public const string Execute_ClickType = "e";

        public const string TotalResolved_ClickType = "tr";
        public const string TotalExecuted_ClickType = "te";

        public const string AvgResolveTime_ClickType = "ar";
        public const string AvgExecuteTime_ClickType = "ae";



        public string HostName { get; set; }
        public string ClickType { get; set; }
        public IClick<double> Next { get; set; }
        public double Value { get; set; }
        public long When { get; set; }
        public object Clone()
        {
            return new RouterClickBase()
            {
                ClickType = this.ClickType,
                Value = this.Value,
                When = this.When
            };
        }

    }
}
