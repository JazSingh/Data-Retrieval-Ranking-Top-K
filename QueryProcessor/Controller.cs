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
            // the result query and k value
            Dictionary<string, List<string>> q = new Dictionary<string, List<string>>();
            int k = -1;

            // split query attributes
            string[] attributes = query.Split(',');
            int n = attributes.Length;

            // derive the query attributes and the k value
            for(int i = 0; i < n; i++)
            {
                string a = attributes[i];

                // remove whitespaces and "
                a = a.Replace(" ", String.Empty);
                a = a.Replace("\"", String.Empty);

                // split attribute
                string[] comps = a.Split('=');

                // filter k
                if (comps[0][0] == 'k')
                    k = Convert.ToInt32(comps[1]);

                // add attribute and value to query
                else
                {
                    if (!q.ContainsKey(comps[0]))
                        q.Add(comps[0], new List<string> { comps[1] });
                    else
                        q[comps[0]].Add(comps[1]);
                }                    
            }

            // if no k value was found; use the defealt value (10)
            if (k == -1)
                k = 10;

            // return query string and k value
            return new Tuple<Dictionary<string, List<string>>, int>(q, k);
        }
    }
}
