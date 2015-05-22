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
        public DatabaseController()
        {
            m_dbConnection = new SQLiteConnection("Data Source=autompg.db;Version=3;");
            m_dbConnection.Open();            
        }

        public string[] Query(string query, int k)
        {
            // result
            string[] ouput = new string[k];

            // Execute Query
            SQLiteCommand command = new SQLiteCommand(query, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            // Proces Ouput To String[]
            for (int i = 0; i < k; i++)
            {
                reader.Read();
                ouput[i] = "ID: " + reader["id"] +
                           "\tType: " + reader["type"] +
                           "\tName: " + reader["model"];
            }

            // Return Query Results
            return ouput;
        }
    }
}
