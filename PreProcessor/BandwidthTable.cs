using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PreProcessor
{
    public class BandwidthTable
    {
        public Dictionary<string, float> table;
        public BandwidthTable()
        {
            table = new Dictionary<string, float>();
        }

        public void Flush(string file)
        {
            string tableName = "Bandwidth";
            int i = 0;
            string[] statements = new string[table.Count];
            foreach (var kvp in table)
            {
                statements[i] = string.Format("INSERT OR REPLACE INTO {0} VALUES (\'{1}\', {2});", tableName, kvp.Key, kvp.Value.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                i++;
            }
            File.AppendAllLines(file, statements);
        }
    }
}
