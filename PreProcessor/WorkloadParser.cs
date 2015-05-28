using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PreProcessor
{
    class WorkloadParser
    {
        int[] freqs;
        string[] queries;
        int numItems = 0;

        int RMaxQF = 0;

        List<Tuple<string, string[], int>> INClauses;

        public WorkloadParser()
        {
            string[] contents = GetRawContents();
            freqs = new int[contents.Length-2];
            queries = new string[contents.Length - 2];
            Extract(contents);
            INClauses = GetINClauses();
        }

        //Load contents from file
        private string[] GetRawContents()
        {
            return File.ReadAllLines("workload.txt");
        }
        
        //Extract Frequencies and Queries
        private void Extract(string[] contents)
        {
            int i = 2;
            while(i < contents.Length && !string.IsNullOrEmpty(contents[i]))
            {
                string[] raw = contents[i].Split(new string[] {" times: "}, StringSplitOptions.None);
                freqs[i-2] = int.Parse(raw[0]);
                queries[i-2] = raw[1];
                i++;
            }
            numItems = i - 2;
        }

        public int CalculateRMaxQF(Dictionary<Tuple<string, string>, int> qfs, string col)
        {
            int max = 0;
            foreach (var kvp in qfs)
                max = kvp.Key.Item1.Equals(col) && kvp.Value > max ? kvp.Value : max;
            return max;
        }

        //Count QF for each attr in the queries
        public Dictionary<Tuple<string, string>, int> ContructAttrValFreqs()
        {
            Dictionary<Tuple<string, string>, int> avf = new Dictionary<Tuple<string, string>, int>();
            for (int i = 0; i < numItems; i++)
            {
                var attrvals = GetAttrVals(queries[i]);
                foreach (var av in attrvals)
                {
                    if (avf.ContainsKey(av)) avf[av] += freqs[i];
                    else avf.Add(av, freqs[i]);
                }
            }
            return avf;
        }

        //Extract attributes and their values from a query
        public List<Tuple<string, string>> GetAttrVals(string query)
        {
            List<Tuple<string, string>> av = new List<Tuple<string, string>>();
            string whereClause = query.Split(new string[] { " WHERE " }, StringSplitOptions.None)[1];
            string[] ands = whereClause.Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string atval in ands)
            {
                string[] split;
                //Type "IN" or "="?
                if (atval.Contains('='))
                {
                    split = atval.Split('=');
                    av.Add(new Tuple<string, string>(split[0], split[1].Replace("\'", string.Empty)));
                }
                else
                {
                    split = atval.Split(new string[] { " IN " }, StringSplitOptions.None);
                    string[] b = split[1]
                        .Replace("(", string.Empty)
                        .Replace(")", string.Empty)
                        .Replace("\'", string.Empty)
                        .Split(',');
                    foreach(string val in b)
                        av.Add(new Tuple<string, string>(split[0], val));
                }
            }
            return av;
        }

        public int SumFreqs()
        {
            int s = 0;
            foreach (int freq in freqs)
                s += freq;
            return s;
        }

        private List<Tuple<string, string[], int>> GetINClauses()
        {                 //att   vals      freq
            var list = new List<Tuple<string, string[], int>>();
            for (int i = 0; i < numItems; i++)
            {
                string query = queries[i];
                string whereClause = query.Split(new string[] { " WHERE " }, StringSplitOptions.None)[1];
                string[] ands = whereClause.Split(new string[] { " AND " }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string atval in ands)
                {
                    string[] split;
                    //Type "IN" or "="?
                    if (atval.Contains('='))
                        continue;
                    else
                    {
                        split = atval.Split(new string[] { " IN " }, StringSplitOptions.None);
                        string[] b = split[1]
                            .Replace("(", string.Empty)
                            .Replace(")", string.Empty)
                            .Replace("\'", string.Empty)
                            .Split(',');
                        list.Add(new Tuple<string, string[], int>(split[0], b, freqs[i]));
                    }
                }
            }
            return list;
        }

        public int PairsTogether(string col, string attr1, string attr2)
        {
            int f = 0;
            foreach (var k in INClauses)
            {
                if (k.Item1 == col)
                {
                    if (k.Item2.Contains(attr1) && k.Item2.Contains(attr2))
                        f += k.Item3;
                }
            }
            return f;
        }
    }
}
