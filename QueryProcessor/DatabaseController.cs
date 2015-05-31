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
        Dictionary<string, Dictionary<string, double[]>> metadb;
        Dictionary<Tuple<string, string, string>, double> overlap;
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
            metadb = new Dictionary<string, Dictionary<string, double[]>>();
            overlap = new Dictionary<Tuple<string, string, string>, double>();

            // metadb variables
            string val = "attr";
            string qfidfval = "qfidf";
            string imp = "importance";

            // variables needed for loading the metadb
            SQLiteCommand command;
            SQLiteDataReader reader;
            Dictionary<string, double[]> tabel;

            // load categorical and numerical data
            foreach (string attr in all_attributes)
            {
                command = new SQLiteCommand("SELECT " + val + ", " + qfidfval + ", " + imp + " FROM " + attr + ";", meta_dbConnection);
                reader = command.ExecuteReader();
                tabel = new Dictionary<string, double[]>();
                while (reader.Read())
                {
                    double[] info = new double[2];
                    info[0] = Convert.ToDouble(reader[qfidfval]);
                    info[1] = Convert.ToDouble(reader[imp]);
                    tabel.Add(reader[val].ToString(), info);
                }
                metadb.Add(attr, tabel);
            } 
         
            // load bandwidth
            command = new SQLiteCommand("SELECT " + val + ", bandwith FROM bandwidth;", meta_dbConnection);
            reader = command.ExecuteReader();
            tabel = new Dictionary<string, double[]>();
            while (reader.Read())
            {
                double[] info = new double[1];
                info[0] = Convert.ToDouble(reader["bandwith"]);
                tabel.Add(reader[val].ToString(), info);
            }
            metadb.Add("bandwidth", tabel);

            // load attribute-overlap
            command = new SQLiteCommand("SELECT col, attr1, attr2, similarity FROM attributeoverlap;", meta_dbConnection);
            reader = command.ExecuteReader();
            tabel = new Dictionary<string, double[]>();
            while (reader.Read())            
                overlap.Add(new Tuple<string, string, string>(reader["col"].ToString(), 
                                                            reader["attr1"].ToString(), 
                                                            reader["attr2"].ToString()), 
                                                            Convert.ToDouble(reader["similarity"]));            
        }

        public string[] ExecuteQuery(Dictionary<string, List<string>> query, int k)
        {
            
            // result
            string[] ouput = new string[k];

            // create a list for each attr based on sim value
            Dictionary<int, double>[] similarity_tables = new Dictionary<int, double>[query.Count];
            int[][] indexes = new int[query.Count][];
            List<string> seenattributes = new List<string>();
            int i = 0;
            foreach (KeyValuePair<string, List<string>> attribute in query)
            {                
                string attr = attribute.Key;
                Dictionary<int, double> similarity_table = new Dictionary<int, double>();

                // numerical
                if (numerical.Contains(attr))
                {
                    // load attr colum from db
                    SQLiteCommand command = new SQLiteCommand("SELECT id, " + attr + " FROM autompg;", m_dbConnection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int key = Convert.ToInt32(reader[0], System.Globalization.NumberFormatInfo.InvariantInfo);
                        double b = Convert.ToDouble(reader[1], System.Globalization.NumberFormatInfo.InvariantInfo);

                        double s = 0;
                        foreach (string val in attribute.Value)
                        {
                            double a = Convert.ToDouble(val, System.Globalization.NumberFormatInfo.InvariantInfo);
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

                        double s = 0;
                        foreach (string a in attribute.Value)
                            s = Math.Max(s, cat_sim(attr, a, b));

                        similarity_table.Add(key, s);
                        //Console.WriteLine("id: " + reader[0] + "\t " + attr + ": " + reader[1]);
                    }
                }

                // add dict to table list (random access)
                similarity_tables[i] = (similarity_table);

                // dict -> sorted array (sorted access)
                List<KeyValuePair<int, double>> sortedlist = similarity_table.ToList();
                // sorts on values from low to high
                sortedlist.Sort(
                    delegate(KeyValuePair<int, double> firstPair,
                    KeyValuePair<int, double> nextPair)
                    {
                        return firstPair.Value.CompareTo(nextPair.Value);
                    }
                );

                // build array
                indexes[i] = new int[similarity_table.Count];
                int j = similarity_table.Count - 1;
                foreach (KeyValuePair<int, double> entry in sortedlist)
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
            List<Tuple<int, double>> topk = TopKSelection(k, indexes, similarity_tables, missingattributes);

            // get the Top-K tuples
            i = 0;
            foreach (Tuple<int,double> entry in topk)
            {
                // get id
                int id = entry.Item1;

                // obtain corresponding tuple
                SQLiteCommand command = new SQLiteCommand("SELECT * FROM autompg WHERE id = " + id + ";", m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                reader.Read();
                ouput[i] = "ID: " + reader["id"] +
                           "\tBrand: " + reader["brand"] +
                           "\tType: " + reader["type"] +
                           "\tModel: " + reader["model"];
                    
                i++;
            }

            return ouput;
        }

        public List<Tuple<int, double>> TopKSelection(int k , int[][] indexes, Dictionary<int, double>[] sim_tabel, List<string> missing_attributes)
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
            Dictionary<int, double[]> found = new Dictionary<int, double[]>();

            // Fagin's alogrithm
            // for row
            for (int i = 0; i < n && s < k; i++)     
                // for column
                for (int j = 0; j < m && s < k; j++)
                {
                    // obtain key and value
                    int key = indexes[j][i];
                    double value = sim_tabel[j][key];

                    // store key and value
                    double[] vs;
                    if (!found.ContainsKey(key))
                        vs = new double[m];
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
            List<Tuple<int, double>> topk = new List<Tuple<int, double>>();
            foreach (KeyValuePair<int, double[]> entry in found)
            {
                // calculate total sim
                double score = 0;
                for (int i = 0; i < m; i++)
                {
                    double x = entry.Value[i];
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
                        topk.Insert(i, new Tuple<int, double>(entry.Key, score));
                        inserted = true;
                        break;
                    }
                    else if (score == topk[i].Item2)
                    {
                        // break tie, using the importance values to calculate the missing attribute scores
                        double score_a = 0;
                        double score_b = 0;

                        foreach (string attr in missing_attributes)
                        {
                            // retrieve attribute values for a and b, and add this value to the importance score
                            SQLiteCommand command = new SQLiteCommand("SELECT " + attr + " FROM autompg WHERE id IN (" + entry.Key + ", " + topk[i].Item1 + ");", m_dbConnection);
                            SQLiteDataReader reader = command.ExecuteReader();
                            
                            reader.Read();
                            string val_a = reader[0].ToString();
                            score_a += metadb[attr][val_a][1];

                            reader.Read();
                            string val_b = reader[0].ToString();
                            score_b += metadb[attr][val_b][1];
                        }

                        // compare the total missing attribute scores
                        if (score_a > score_b)
                        {
                            topk.Insert(i, new Tuple<int, double>(entry.Key, score));
                            inserted = true;
                            break;
                        }
                    }
                }
                // if not inserted and the topk is incomplete then add x to the end of the topk list
                if (topk.Count < k && !inserted)
                    topk.Add(new Tuple<int, double>(entry.Key, score));

                // if there are to much elements in the topk list, remove the last (lowest score)
                if (topk.Count > k)                
                    topk.RemoveAt(k);
            }

            // return sorted id with length k
            return topk;
        }        

        // Numerical Similarity
        public double num_sim(string attr, double a, double b)
        {
            if (a == b)
            {
                double res = metadb[attr][a.ToString()][0];
                return metadb[attr][a.ToString()][0];
            }
            else
            {
                double h = metadb["bandwidth"][attr][0];
                //qfidf(b)
                double b_weight = metadb[attr][b.ToString()][0];
                double res = Math.Pow(Math.E, -0.5 * Math.Pow(((a - b) / h), 2)) * b_weight;
                return Math.Pow(Math.E, -0.5 * Math.Pow(((a - b) / h), 2)) * b_weight;
            }
        }

        // Categorical Similarity
        public double cat_sim(string attr, string a, string b)
        {
            if (a == b)
                return metadb[attr][a][0];
            else
                if (overlap.ContainsKey(new Tuple<string, string, string>(attr, a, b)))
                    return overlap[new Tuple<string, string, string>(attr, a, b)];
                else if (overlap.ContainsKey(new Tuple<string, string, string>(attr, b, a)))
                    return overlap[new Tuple<string, string, string>(attr, b, a)];
                else
                    return 0;
        }
    }
}
