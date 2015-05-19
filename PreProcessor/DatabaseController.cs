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
            //string sql = "SELECT * FROM autompg";
            //SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            //SQLiteDataReader reader = command.ExecuteReader();
            //while (reader.Read())
            //    Console.WriteLine("ID: " + reader["id"] + "\tType: " + reader["type"]);
        }
    }
}
