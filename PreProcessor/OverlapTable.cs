using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreProcessor
{
    public class OverlapTable
    {
        public Dictionary<Tuple<string, string, string>, float> table;

        public OverlapTable()
        {
            table = new Dictionary<Tuple<string, string, string>, float>();
        }
    }
}
