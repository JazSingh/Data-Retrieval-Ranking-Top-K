using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyseP1
{
    class Controller
    {
        public Controller()
        {
            DatabaseController DBController = new DatabaseController();

            while (true)
            {
                // >>
                Console.Write(">> ");

                // Read Input
                string q = Console.ReadLine();

                // Proces Input
                if (q != "")
                {
                    // Parse Query
                    Tuple<Dictionary<string, List<string>>, int> res = ParseQuery(q);
                    Dictionary<string, List<string>> query = res.Item1;
                    int k = res.Item2;

                    if (query.Count > 0)
                    {
                        // Execute Query
                        string[] output = DBController.ExecuteQuery(query, k);

                        // Return Output
                        foreach (string line in output)
                            Console.WriteLine(line);
                    }
                    else
                        // won't happen yet...
                        Console.WriteLine("Invalid Query!");
                }                
            }
        }

        public Tuple<Dictionary<string, List<string>>, int> ParseQuery(string query)
        {
            // pre-parse operations
            // remove whitespaces and "
            query = query.Replace(" ", String.Empty);
            query = query.Replace("\"", String.Empty);
            query = query.Replace("\'", String.Empty);

            // the result query and k value
            Dictionary<string, List<string>> q = new Dictionary<string, List<string>>();
            int k = -1;
            int i = 0;

            while (i < query.Length)
            {
                string attr = "";
                List<string> values = new List<string>();
                
                // read attribute
                while (query[i] != '=' && (query[i] != 'I' && query[i + 1] != 'N'))
                {
                    attr += query[i];
                    i++;
                }
                if (query[i] == '=')
                {
                    // read value
                    string val = "";
                    i++;
                    while (query[i] != ',' && query[i] != ';')
                    {
                        val += query[i];
                        i++;
                    }
                    values.Add(val);
                }
                else if (query[i] == 'I' && query[i + 1] == 'N')
                {
                    while (query[i] != '(')
                        i++;
                    // read value's
                    i++; // skip '('
                    while (query[i] != ',' && query[i] != ';')
                    {
                        string val = "";
                        while (query[i] != ',' && query[i] != ')')
                        {
                            val += query[i];
                            i++;
                        }
                        values.Add(val);
                        i++;
                    }
                }

                // top-k value
                if (attr == "k")
                    k = Convert.ToInt32(values[0]);
                else
                {
                    // store attribute and value
                    if (!q.ContainsKey(attr))                    
                        q.Add(attr, values);
                    
                    else
                        foreach (string str in values)
                            q[attr].Add(str);
                }

                // move till end or next attr and value
                while (i < query.Length && query[i] != ',' && query[i] != ';')
                    i++;
                i++;
            }

            // if no k value was found; use the defealt value (10)
            if (k == -1)
                k = 10;

            // return query string and k value
            return new Tuple<Dictionary<string, List<string>>, int>(q, k);
        }
    }
}
