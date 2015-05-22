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

        //TODO
        public Dictionary<string, int> GetCatData(string col)
        {
            throw new NotImplementedException();
            string sql = string.Format("SELECT {0}, count(*) as \"tel\" FROM autompg GROUP BY {0}", col);
            SQLiteDataReader reader = ExecuteGetQuery(sql);
            while (reader.Read())
                Console.WriteLine(col + ": " + reader[col] + "\tCount: " + reader["tel"]);
            return null;
        }

        public List<string> GetDistinctAttributeValues(string col)
        {
            string sql = string.Format("SELECT DISTINCT {0} FROM autompg", col);
            SQLiteDataReader reader = ExecuteGetQuery(sql);
            List<string> vals = new List<string>();
            while (reader.Read())
                vals.Add(reader[col].ToString());
            return vals;
        }

        private SQLiteDataReader ExecuteGetQuery(string sql)
        {
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            return command.ExecuteReader();
        }
    }
}
