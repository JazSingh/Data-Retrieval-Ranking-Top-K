using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace DataAnalyseP1
{
    class DatabaseController
    {
        SQLiteConnection m_dbConnection;
        Dictionary<string, Dictionary<string, Tuple<float, float>>> metadb;
        public DatabaseController()
        {
            // setup database connection
            m_dbConnection = new SQLiteConnection("Data Source=autompg.db;Version=3;");
            m_dbConnection.Open();

            /*
            // load metadb to dictionary
            SQLiteConnection meta_dbConnection = new SQLiteConnection("Data Source=meta.db;Version=3;");
            meta_dbConnection.Open();

            // metadb variables
            string val = "val";
            string qfidfval = "qfidfval";
            string imp = "importance";

            // Type
            SQLiteCommand command = new SQLiteCommand("SELECT " + val + ", " + qfidfval + ", " + imp + " FROM type;", meta_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            Dictionary<string, Tuple<float, float>> type = new Dictionary<string, Tuple<float, float>>();
            while (reader.Read())            
                type.Add((string)reader[0], new Tuple<float, float>((float)reader[1], (float)reader[2]));
            metadb.Add("type", type);

            // ...
            */
            
        }

        public string[] ExecuteQuery(Dictionary<string, List<string>> query, int k)
        {
            
            // result
            string[] ouput = new string[k];

            // create a list for each attr based on sim value
            Dictionary<int, float>[] similarity_tables = new Dictionary<int, float>[query.Count];
            int[][] indexes = new int[query.Count][];
            List<string> numerical = new List<string>() { "id", "mpg", "cylinders", "displacement", "horsepower", "weight", "accelaration", "model_year", "origin"};
            List<string> categorical = new List<string>() { "brand", "model", "type" };
            List<string> seenattributes = new List<string>();
            int i = 0;
            foreach (KeyValuePair<string, List<string>> attribute in query)
            {                
                string attr = attribute.Key;
                Dictionary<int, float> similarity_table = new Dictionary<int, float>();

                // numerical
                if (numerical.Contains(attr))
                {
                    // load attr colum from db
                    SQLiteCommand command = new SQLiteCommand("SELECT id, " + attr + " FROM autompg;", m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int key = Convert.ToInt32(reader[0]);
                        int b = Convert.ToInt32(reader[1]);

                        float s = 0;
                        foreach (string val in attribute.Value)
                        {
                            int a = Convert.ToInt32(val);
                            s = Math.Max(s, num_sim(attr, a, b));
                        }

                        similarity_table.Add(key, s);
                        //Console.WriteLine("id: " + reader[0] + "\t " + attr + ": " + reader[1]);
                    }
                }

                // categorical
                else if (categorical.Contains(attr))
                    foreach (string value in attribute.Value)
                    {
                        // load attr colum from db
                        SQLiteCommand command = new SQLiteCommand("SELECT id, " + attr + " FROM autompg;", m_dbConnection);
                        SQLiteDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            int key = Convert.ToInt32(reader[0]);
                            string b = (string)reader[1];

                            float s = 0;
                            foreach (string a in attribute.Value)
                                s = Math.Max(s, cat_sim(attr, a, b));                          

                            similarity_table.Add(key, s);
                            //Console.WriteLine("id: " + reader[0] + "\t " + attr + ": " + reader[1]);
                        }
                    }

                // add dict to table list (random access)
                similarity_tables[i] = (similarity_table);

                // dict -> sorted array (sorted access)
                List<KeyValuePair<int, float>> sortedlist = similarity_table.ToList();
                // sorts on values from low to high
                sortedlist.Sort(
                    delegate(KeyValuePair<int, float> firstPair,
                    KeyValuePair<int, float> nextPair)
                    {
                        return firstPair.Value.CompareTo(nextPair.Value);
                    }
                );
                // build array
                indexes[i] = new int[similarity_table.Count];
                int j = similarity_table.Count - 1;
                foreach (KeyValuePair<int, float> entry in sortedlist)
                {
                    indexes[i][j] = entry.Key;
                    j--;
                }

                // update seen and index
                seenattributes.Add(attr);
                i++;
            }
            
            // ...

            // obtain importance values for missing attributes
            List<string> missingattributes = new List<string>();
            foreach (string elem in numerical)            
                if (!(seenattributes.Contains(elem)))
                    missingattributes.Add(elem);
            foreach (string elem in categorical)
                if (!(seenattributes.Contains(elem)))
                    missingattributes.Add(elem);            
            // ...

            // get the Top-K ID's
            List<Tuple<int, float>> topk = TopKSelection(k);

            // get the Top-K tuples
            i = 0;
            foreach (Tuple<int,float> entry in topk)
            {
                // get id
                int id = entry.Item1;

                // obtain corresponding tuple
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM autompg WHERE id = " + id + ";", m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                reader.Read();
                ouput[i] = "ID: " + reader["id"] +
                           "\tType: " + reader["type"] +
                           "\tName: " + reader["model"];
                    
                i++;
            }

            return ouput;
        }

        public List<Tuple<int, float>> TopKSelection(int k) // , int[][] indexes, Dictionary<int, float>[] sim_tabel, Dictionary<int, float>[] imp_tabel)
        {
            // Top-K selection using Fagin's Algorithm
            int m = 3;
            // retrieve attribute columns: <id, similarity value>, and an index array sorted by similarity value
            // ...
            // def indexes
            int[][] indexes = new int[m][];
            indexes[0] = new int[] { 3, 2, 4, 1, 5 };
            indexes[1] = new int[] { 2, 1, 4, 3, 5 };
            indexes[2] = new int[] { 1, 2, 3, 5, 4 };

            // def dict
            Dictionary<int, float>[] tabel = new Dictionary<int, float>[m];
            tabel[0] = new Dictionary<int, float>();
            tabel[0].Add(3, 0.8f);
            tabel[0].Add(2, 0.7f);
            tabel[0].Add(4, 0.5f);
            tabel[0].Add(1, 0.4f);
            tabel[0].Add(5, 0.2f);
            tabel[1] = new Dictionary<int, float>();
            tabel[1].Add(2, 0.9f);
            tabel[1].Add(1, 0.85f);
            tabel[1].Add(4, 0.5f);
            tabel[1].Add(3, 0.4f);
            tabel[1].Add(5, 0.3f);
            tabel[2] = new Dictionary<int, float>();
            tabel[2].Add(1, 0.95f);
            tabel[2].Add(2, 0.9f);
            tabel[2].Add(3, 0.8f);
            tabel[2].Add(5, 0.2f);
            tabel[2].Add(4, 0.1f);

            // ...
            // n = number of id's / tupels
            int n = 5;

            // number of id's seen in every attribute list (m)
            int s = 0;
            // tabel to keep track of seen attributes
            Dictionary<int, int> seen = new Dictionary<int, int>();
            // tabel to keep the found similarity results
            Dictionary<int, float[]> found = new Dictionary<int, float[]>();

            // Fagin
            // for row
            for (int i = 0; i < n && s < k; i++)     
                // for column
                for (int j = 0; j < m && s < k; j++)
                {
                    // obtain key and value
                    int key = indexes[j][i];
                    float value = tabel[j][key];

                    // store key and value
                    float[] vs;
                    if (!found.ContainsKey(key))
                        vs = new float[m];
                    else
                        vs = found[key];                        
                    vs[j] = value;
                    found[key] = vs;

                    // mark key as seen
                    int x;
                    if (!seen.ContainsKey(key))
                        x = 0;
                    else 
                        x = seen[key];
                    x++;
                    if (x == m)
                        s++;
                    seen[key] = x;                    
                }            
            
            // calculate total similarity (retrieve value where necessary), and insert in topk list (ordered by scoring)
            List<Tuple<int, float>> topk = new List<Tuple<int, float>>();
            foreach (KeyValuePair<int, float[]> entry in found)
            {
                // calculate total sim
                float score = 0;
                for (int i = 0; i < m; i++)
                {
                    float x = entry.Value[i];
                    if (x == 0.0f)
                        x = tabel[i][entry.Key];
                    score += x;
                }
              
                // insert in topk
                bool inserted = false;
                for (int i = 0; i < topk.Count; i++)
                {
                    // if score x is higher than the score at index i, insert x
                    if (score > topk[i].Item2)
                    {
                        topk.Insert(i, new Tuple<int, float>(entry.Key, score));
                        inserted = true;
                        break;
                    }
                    else if (score == topk[i].Item2)
                    {
                        // break tie
                        // calculate missing attribute score
                        // ...
                    }
                }
                // if not inserted and the topk is incomplete then add x to the end of the topk list
                if (topk.Count < k && !inserted)
                    topk.Add(new Tuple<int, float>(entry.Key, score));

                // if there are to much elements in the topk list, remove the last (lowest score)
                if (topk.Count > k)                
                    topk.RemoveAt(k);
            }

            // return sorted id with length k
            return topk;
        }        

        // Numerical Similarity
        public float num_sim(string attr, int a, int b)
        {
            return 1;
        }

        // Categorical Similarity
        public float cat_sim(string attr, string a, string b)
        {
            return 1;
        }
    }
}
