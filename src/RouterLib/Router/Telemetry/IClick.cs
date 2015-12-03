using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterLib
{
    public interface IClick<ValueT>
    {
        long When { get; set; }

        IClick<ValueT> Next { get; set; }

        ValueT Value { get; set; }
    }
}
