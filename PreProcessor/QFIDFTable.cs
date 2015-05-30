using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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


        public abstract void Flush(string file);
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

        public override void Flush(string file)
        {
            string tableName = GetName();
            int i = 0;
            string[] statements = new string[table.Count];
            foreach(var kvp in table)
            {
                statements[i] = string.Format("INSERT OR REPLACE INTO {0} VALUES (\'{1}\', {2}, {3});", tableName, kvp.Key, kvp.Value.GetQFIDF().ToString(System.Globalization.NumberFormatInfo.InvariantInfo), kvp.Value.GetImportance().ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                i++;
    }
            File.AppendAllLines(file, statements);
        }

        private string GetName()
        {
            switch (Name)
            {
                case "type": return "Type";
                case "model": return "Model";
                case "brand": return "Brand";
            }
            throw new Exception();
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

        public override void Flush(string file)
        {
            string tableName = GetName();
            int i = 0;
            string[] statements = new string[table.Count];
            foreach (var kvp in table)
            {
                statements[i] = string.Format("INSERT OR REPLACE INTO {0} VALUES ({1}, {2}, {3});", tableName, kvp.Key.ToString(System.Globalization.NumberFormatInfo.InvariantInfo), kvp.Value.GetQFIDF().ToString(System.Globalization.NumberFormatInfo.InvariantInfo), kvp.Value.GetImportance().ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
                i++;
            }
            File.AppendAllLines(file, statements);
        }

        private string GetName()
        {
            switch (Name)
            {
                case "origin": return "Origin";
                case "model_year": return "ModelYear";
                case "acceleration": return "Acceleration";
                case "weight": return "Weight";
                case "horsepower": return "Horsepower";
                case "displacement": return "Displacement";
                case "cylinders": return "Cylinders";
                case "mpg": return "Mpg";
            }
            throw new Exception();
        }
    }
}
