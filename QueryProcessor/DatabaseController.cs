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
        Dictionary<string, Dictionary<string, float[]>> metadb;
        List<string> numerical;
        List<string> categorical;
        List<string> all_attributes;

        public DatabaseController()
        {
            // setup database connection
            m_dbConnection = new SQLiteConnection("Data Source=autompg.db;Version=3;");
            m_dbConnection.Open();            
            
            // load metadb to dictionary
            SQLiteConnection meta_dbConnection = new SQLiteConnection("Data Source=metadb.db;Version=3;");
            meta_dbConnection.Open();

            // attributes
            numerical = new List<string>() { "mpg", "cylinders", "displacement", "horsepower", "weight", "acceleration", "model_year", "origin" };
            categorical = new List<string>() { "brand", "model", "type" };
            all_attributes = new List<string>() { "mpg", "cylinders", "displacement", "horsepower", "weight", "acceleration", "model_year", "origin", "brand", "model", "type" };
            
            // metadb
            metadb = new Dictionary<string, Dictionary<string, float[]>>();

            // metadb variables
            string val = "attr";
            string qfidfval = "qfidf";
            string imp = "importance";

            // load categorical and numerical data
            foreach (string attr in all_attributes)
            {
                SQLiteCommand command = new SQLiteCommand("SELECT " + val + ", " + qfidfval + ", " + imp + " FROM " + attr + ";", meta_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                Dictionary<string, float[]> tabel = new Dictionary<string, float[]>();
                while (reader.Read())
                {
                    float[] info = new float[2];
                    info[0] = (float)Convert.ToDouble(reader[qfidfval]);
                    info[1] = (float)Convert.ToDouble(reader[imp]);
                    tabel.Add(reader[val].ToString(), info);
                }
                metadb.Add(attr, tabel);
            }          

        }

        public string[] ExecuteQuery(Dictionary<string, List<string>> query, int k)
        {
            
            // result
            string[] ouput = new string[k];

            // create a list for each attr based on sim value
            Dictionary<int, float>[] similarity_tables = new Dictionary<int, float>[query.Count];
            int[][] indexes = new int[query.Count][];
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
                        int key = Convert.ToInt32(reader[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                        int b = Convert.ToInt32(reader[1]);

                        float s = 0;
                        foreach (string val in attribute.Value)
                        {
                            int a = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);
                            s = Math.Max(s, num_sim(attr, a, b));
                        }

                        similarity_table.Add(key, s);
                        //Console.WriteLine("id: " + reader[0] + "\t " + attr + ": " + reader[1]);
                    }
                }

                // categorical
                else if (categorical.Contains(attr))
                {
                    // load attr colum from db
                    SQLiteCommand command = new SQLiteCommand("SELECT id, " + attr + " FROM autompg;", m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int key = Convert.ToInt32(reader[0], System.Globalization.NumberFormatInfo.InvariantInfo);
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

            // obtain missing attributes
            List<string> missingattributes = new List<string>();
            foreach (string elem in numerical)            
                if (!(seenattributes.Contains(elem)))
                    missingattributes.Add(elem);
            foreach (string elem in categorical)
                if (!(seenattributes.Contains(elem)))
                    missingattributes.Add(elem);           

            // get the Top-K ID's
            List<Tuple<int, float>> topk = TopKSelection(k, indexes, similarity_tables, missingattributes);

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

        public List<Tuple<int, float>> TopKSelection(int k , int[][] indexes, Dictionary<int, float>[] sim_tabel, List<string> missing_attributes)
        {
            // --- Top-K selection using Fagin's Algorithm ---
            // m = number of attribute tables
            int m = sim_tabel.Length;
            
            // n = number of id's / tupels
            int n = sim_tabel[0].Count;

            // number of id's seen in every attribute list (m)
            int s = 0;
            
            // tabel to keep track of seen attributes
            Dictionary<int, int> seen = new Dictionary<int, int>();
            
            // tabel to keep the found similarity results
            Dictionary<int, float[]> found = new Dictionary<int, float[]>();

            // Fagin's alogrithm
            // for row
            for (int i = 0; i < n && s < k; i++)     
                // for column
                for (int j = 0; j < m && s < k; j++)
                {
                    // obtain key and value
                    int key = indexes[j][i];
                    float value = sim_tabel[j][key];

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
                        x = sim_tabel[i][entry.Key];
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
                        // break tie, using the importance values to calculate the missing attribute scores
                        float score_a = 0;
                        float score_b = 0;

                        foreach (string attr in missing_attributes)
                        {
                            // retrieve attribute values for a and b, and add this value to the importance score
                            SQLiteCommand command = new SQLiteCommand("SELECT " + attr + " FROM autompg WHERE id IN (" + entry.Key + ", " + topk[i].Item1 + ");", m_dbConnection);
                            SQLiteDataReader reader = command.ExecuteReader();
                            
                            reader.Read();
                            string val_a = reader[0].ToString();
                            //score_a += metadb[attr][val_a][1];

                            reader.Read();
                            string val_b = reader[0].ToString();
                            //score_b += metadb[attr][val_b][1];
                        }

                        // compare the total missing attribute scores
                        if (score_a > score_b)
                        {
                            topk.Insert(i, new Tuple<int, float>(entry.Key, score));
                            inserted = true;
                            break;
                        }
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
            //float h = metadb["bandwidth"][attr];
            //qfidf(b)
            //float b_weight = metadb["attr"][b][0]

            // return Math.Pow(Math.E, 0.5 * ((a - b) / h) * ((a - b) / h)) * b_weight
            return 1;
        }

        // Categorical Similarity
        public float cat_sim(string attr, string a, string b)
        {
            /*
            if (a == b)
                return metadb[attr][b][0]
            else
                return metadb["overlap"][a][0]
            */
            return 1;
        }
    }
}
