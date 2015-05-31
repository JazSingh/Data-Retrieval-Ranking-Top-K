using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PreProcessor
{
    public class OverlapTable
    {
        public Dictionary<Tuple<string, string, string>, double> table;

        public OverlapTable()
        {
            table = new Dictionary<Tuple<string, string, string>, double>();
        }

        public void Flush(string file)
        {
            string tableName = "AttributeOverlap";
            int i = 0;
            string[] statements = new string[table.Count];
            foreach (var kvp in table)
            {
                statements[i] = string.Format("INSERT OR REPLACE INTO {0} VALUES (\'{1}\', \'{2}\', \'{3}\', {4});"
                    , tableName, kvp.Key.Item1, kvp.Key.Item2.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), kvp.Key.Item3.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), kvp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                i++;
            }
            File.AppendAllLines(file, statements);
        }
    }
}
