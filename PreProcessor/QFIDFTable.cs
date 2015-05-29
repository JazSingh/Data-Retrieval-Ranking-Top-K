using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreProcessor
{
    public abstract class QFIDFTable
    {
        public readonly string Name;

        public QFIDFTable(string n)
        {
            Name = n;
        }

        public Dictionary<string, Attribute> table;
        public abstract void Initialize(List<string> vals);

        public void SetQF(Dictionary<Tuple<string, string>, int> rqfs, Dictionary<string, int> rmaxqfs)
        {
            int max = rmaxqfs[Name];
            foreach (var kvp in table)
            {
                int k = 0;
                rqfs.TryGetValue(new Tuple<string, string>(Name, kvp.Key), out k);
                table[kvp.Key].SetQF(k, max);
            }
        }
    }

    public class QFIDFCatTable : QFIDFTable
    {
        public QFIDFCatTable(string n) : base(n) { }

        public override void Initialize(List<string> vals)
        {
            table = new Dictionary<string, Attribute>(vals.Count);
            foreach (string val in vals)
                table.Add(val, new CatAttribute(val));
        }

    }

    public class QFIDFNumTable : QFIDFTable
    {
        public QFIDFNumTable(string n) : base(n) { }
        public override void Initialize(List<string> vals)
        {
            table = new Dictionary<string, Attribute>(vals.Count);
            foreach (string val in vals)
                table.Add(val, new NumAttribute(val));
        }
    }
}
