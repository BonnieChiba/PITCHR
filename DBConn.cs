using MySqlConnector;
using System;
using System.Data.SqlClient;

namespace PITCHR
{
    public class DBConn : IDisposable
    {

        public MySqlConnection dbConnection;

        public MySqlCommand dbCommand;

        public DBConn()
        {
            this.CreateConnection();
        }

        public DBConn(string query)
        {
            this.CreateConnection();
            dbCommand = new MySqlCommand(query, dbConnection);
        }

        public bool TestConn()
        {
            try
            {
                dbConnection.Open();
                dbConnection.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose() { dbConnection.Dispose(); }

        private void CreateConnection()
        {
            string dblocation = "localhost";
            string user = "root";
            string password = "";
            string dbName = "integrative";

            string connectionString = "Server=" + dblocation + ";Database=" + dbName + ";Uid=" + user + ";Pwd=" + password + ";";

            //string connectionString = $"Server={ dblocation};Database={ dbName};Uid={ user};Pwd ={  password };";
            dbConnection = new MySqlConnection(connectionString);
            dbConnection.ConnectionString = connectionString;
        }
    }
}