
namespace Common.DataAccess
{
    #region Using

    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
    using Wellsfargo.E2E.Common.ExceptionHandling;
    using Wellsfargo.E2E.Common.Interfaces;
  
    #endregion

    /// <summary>
    /// This class contains method to interact with the SQL database.
    /// </summary>
    public class DataManager : IDataAccess, IDisposable
    {
        #region Member Variables

        /// <summary>
        /// Database object
        /// </summary>
        private SqlDatabase database;

        /// <summary>
        /// Connection object
        /// </summary>
        private SqlConnection connection;

        /// <summary>
        /// A reference to the settings.
        /// </summary>
        private ConnectionSettings settings;

        /// <summary>
        /// Track whether Dispose has been called.
        /// </summary>
        private bool disposed;

        #endregion

        #region Constructor & Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DataManager"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public DataManager(ConnectionSettings settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Finalizes an instance of the DataManager class. This destructor will run only if 
        /// the Dispose method does not get called.
        /// </summary>
        ~DataManager()
        {
            // Call Dispose(false) to dispose all managed and unmanaged resources,
            // which is optimal in terms of maintainability and readability.
            this.Dispose(false);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the sql connection.
        /// </summary>
        /// <value>The connection.</value>
        public SqlConnection Connection
        {
            get
            {
                if (this.connection == null || this.connection.State != ConnectionState.Open)
                {
                    this.GetConnection();
                }

                return this.connection;
            }
        }

        #endregion

        #region Public Members
        /// <summary>
        /// This method executes query and returns result in datatable.
        /// </summary>
        /// <param name="cmdType">command type</param>
        /// <param name="cmdText">command text</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns dataset</returns>
        public DataSet ExecQueryForDataSet(CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {            
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(CommandType.StoredProcedure, cmdText, cmdParameters);
            
            SqlDataAdapter adapter = new SqlDataAdapter(objCmd);
            DataSet dataset = new DataSet();

            try
            {
                adapter.Fill(dataset);
                dataset.Locale = Thread.CurrentThread.CurrentCulture;
                return dataset;
            }         
            finally
            {
                this.Cleanup(objCmd);
                adapter.Dispose();
            }
        }

        /// <summary>
        /// This method executes query and returns result in datatable.
        /// </summary>
        /// <param name="transSQL">Sql Transaction</param>
        /// <param name="cmdType">command type</param>
        /// <param name="cmdText">command text</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns dataset</returns>
        public DataSet ExecQueryForDataSet(SqlTransaction transSQL, CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(transSQL, CommandType.StoredProcedure, cmdText, cmdParameters);
            SqlDataAdapter adapter = new SqlDataAdapter(objCmd);
            DataSet dataset = new DataSet();
            try
            {
                adapter.Fill(dataset);
                dataset.Locale = Thread.CurrentThread.CurrentCulture;
                return dataset;
            }
            finally
            {
                Cleanup(true, objCmd);
                adapter.Dispose();
            }
        }

        /// <summary>
        /// This method executes query and returns result in datatable.
        /// </summary>
        /// <param name="cmdType">command type</param>
        /// <param name="cmdText">command text</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns datatable</returns>
        public DataTable ExecQueryForDataTable(CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {            
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(CommandType.StoredProcedure, cmdText, cmdParameters);
            SqlDataAdapter adapter = new SqlDataAdapter(objCmd);
            try
            {
                var dataTable = new DataTable();
                dataTable.Locale = Thread.CurrentThread.CurrentCulture;
                adapter.Fill(dataTable);

                return dataTable;
            }
            finally
            {
                this.Cleanup(objCmd);
                adapter.Dispose();
            }
        }

        /// <summary>
        /// This method executes query and returns result in datatable.
        /// </summary>
        /// <param name="transSQL">Sql Transaction</param>
        /// <param name="cmdType">command type</param>
        /// <param name="cmdText">command text</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns datatable</returns>
        public DataTable ExecQueryForDataTable(SqlTransaction transSQL, CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(transSQL, CommandType.StoredProcedure, cmdText, cmdParameters);

            SqlDataAdapter adapter = new SqlDataAdapter(objCmd);
            try
            {
                var dataTable = new DataTable();
                dataTable.Locale = Thread.CurrentThread.CurrentCulture;
                adapter.Fill(dataTable);

                return dataTable;
            }
            finally
            {
                Cleanup(true, objCmd);
                adapter.Dispose();
            }
        }

        /// <summary>
        /// This method executes query and returns result in data reader.
        /// </summary>
        /// <param name="cmdType">command type</param>
        /// <param name="cmdText">command text</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns datatable</returns>
        public SqlDataReader ExecReader(CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {            
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(CommandType.StoredProcedure, cmdText, cmdParameters);
            try
            {
                return objCmd.ExecuteReader();
            }
            finally
            {
                this.Cleanup(objCmd);
            }
        }

        /// <summary>
        /// This method executes query and returns result in data reader.
        /// </summary>
        /// <param name="transSQL">Sql Transaction</param>
        /// <param name="cmdType">command type</param>
        /// <param name="cmdText">command text</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns datatable</returns>
        public SqlDataReader ExecReader(SqlTransaction transSQL, CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(transSQL, CommandType.StoredProcedure, cmdText, cmdParameters);
            
            try
            {
                return objCmd.ExecuteReader();
            }
            finally
            {
                Cleanup(true, objCmd);
            }
        }

        /// <summary>
        /// This method execute query and returns first column of the first row and ignores other 
        /// column and row are ignored.
        /// </summary>
        /// <param name="cmdType">command type of command as CommandType instance.</param>
        /// <param name="cmdText">command which need to be execute.</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>no of rows affected</returns>
        public object ExecScalar(CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {            
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(CommandType.StoredProcedure, cmdText, cmdParameters);
            try
            {
                object returnValue = objCmd.ExecuteScalar();

                return returnValue;
            }
            finally
            {
                this.Cleanup(objCmd);
            }
        }

        /// <summary>
        /// This method execute query and returns first column of the first row and ignores other 
        /// column and row are ignored.
        /// </summary>
        /// <param name="transSQL">Current Transaction</param>        
        /// <param name="cmdType">command type of command as CommandType instance.</param>
        /// <param name="cmdText">command which need to be execute.</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>no of rows affected</returns>
        public object ExecScalar(SqlTransaction transSQL, CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(transSQL, CommandType.StoredProcedure, cmdText, cmdParameters);
            try
            {
                object returnValue = objCmd.ExecuteScalar();

                return returnValue;
            }
            finally
            {
                Cleanup(true, objCmd);
            }
        }

        /// <summary>
        /// This method execute query and returns no of rows affected.
        /// </summary>
        /// <param name="cmdType">command type of command as CommandType instance.</param>
        /// <param name="cmdText">command which need to be execute.</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>no of rows affected</returns>
        public int ExecNonQuery(CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {            
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(CommandType.StoredProcedure, cmdText, cmdParameters);
            try
            {
                int returnValue = objCmd.ExecuteNonQuery();

                return returnValue;
            }
            finally
            {
                this.Cleanup(objCmd);
            }
        }

        /// <summary>
        /// This method execute query and returns no of rows affected inside a transaction
        /// </summary>        
        /// <param name="transSQL">The sql transaction object.</param>
        /// <param name="cmdType">command type of command as CommandType instance.</param>
        /// <param name="cmdText">command which need to be execute.</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>no of rows affected</returns>
        public int ExecNonQuery(SqlTransaction transSQL, CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            SqlCommand objCmd = this.CreatesqDatabaseCommandObject(transSQL, CommandType.StoredProcedure, cmdText, cmdParameters);

                int returnValue = objCmd.ExecuteNonQuery();

                if (cmdParameters[cmdParameters.Length - 1].Direction == ParameterDirection.ReturnValue)
                {
                    returnValue = (int)cmdParameters[cmdParameters.Length - 1].Value;
                }

                return returnValue;
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method. Therefore, you should call 
            // GC.SupressFinalize to take this object off the finalization queue and prevent 
            // finalization code for this object from executing a second time.
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private Members

        /// <summary>
        /// This method is used to clean up command and database connection with transaction
        /// </summary>
        /// <param name="flag">if set to <c>true</c> [flag].</param>
        /// <param name="cmd">command object</param>
        private static void Cleanup(bool flag, SqlCommand cmd)
        {
            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        /// <summary>
        /// This method creates and opens sql connection instance.
        /// </summary>
        private void GetConnection()
        {
            this.database = new SqlDatabase(this.settings.ConnectionString);
            this.connection = (SqlConnection)this.database.CreateConnection();

            if (this.connection.State == ConnectionState.Closed)
            {
                this.connection.Open();
            }

            return;
        }

        /// <summary>
        /// This method creates sql command instance.
        /// </summary>
        /// <param name="cmdType">command type of command as CommandType instance.</param>
        /// <param name="cmdText">command which need to be execute.</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns sqlcommand instance</returns>
        private SqlCommand CreatesqDatabaseCommandObject(CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            if (string.IsNullOrEmpty(cmdText))
            {
                throw new BusinessException("Command text cannot be null or empty.");
            }

            SqlCommand command = this.ReadUncommitted();

            command.CommandText = cmdText;
            command.CommandType = cmdType;
            command.CommandTimeout = 60;

            if (cmdParameters != null)
            {
                foreach (SqlParameter parm in cmdParameters)
                {
                    command.Parameters.Add(parm);
                }
            }
        
            return command;
        }

        /// <summary>
        /// Reads the uncommitted.
        /// </summary>
        /// <returns>It will return SQL Command</returns>
        private SqlCommand ReadUncommitted()
        {
            SqlCommand command = new SqlCommand();
            command.Connection = this.Connection;
            command.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
            return command;
        }

        /// <summary>
        /// This method creates sql command instance.
        /// </summary>
        /// <param name="transSQL">The transaction.</param>
        /// <param name="cmdType">command type of command as CommandType instance.</param>
        /// <param name="cmdText">command which need to be execute.</param>
        /// <param name="cmdParameters">store procedure parameters</param>
        /// <returns>returns sqlcommand instance</returns>
        private SqlCommand CreatesqDatabaseCommandObject(SqlTransaction transSQL, CommandType cmdType, string cmdText, params SqlParameter[] cmdParameters)
        {
            if (string.IsNullOrEmpty(cmdText))
            {
                throw new BusinessException("Command text cannot be null or empty.");
            }

            SqlCommand command = this.ReadUncommitted(transSQL);
            command.Transaction = transSQL;
            command.Connection = this.Connection;
            command.CommandText = cmdText;
            command.CommandType = cmdType;

            if (cmdParameters != null)
            {
                foreach (SqlParameter parm in cmdParameters)
                {
                    command.Parameters.Add(parm);
                }
            }

            return command;
        }

        /// <summary>
        /// Reads the uncommitted.
        /// </summary>
        /// <param name="transSQL">The trans SQL.</param>
        /// <returns>this will return SQL command</returns>
        private SqlCommand ReadUncommitted(SqlTransaction transSQL)
        {
            SqlCommand command = new SqlCommand();
            command.Connection = this.Connection;
            command.Transaction = transSQL;  
            command.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;";
            command.CommandType = CommandType.Text;
            command.ExecuteNonQuery();
            return command;
        }

        /// <summary>
        /// This method is used to clean up command and database connection
        /// </summary>
        /// <param name="cmd">command object</param>        
        private void Cleanup(SqlCommand cmd)
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
                this.connection.Dispose();
            }

            cmd.Parameters.Clear();
            cmd.Dispose();
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">true if dispose </param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.connection != null)
                    {
                        this.connection.Close();
                        this.connection.Dispose();
                        this.connection = null;
                    }
                }

                // set the variable to indicate that disposing has been done.
                this.disposed = true;
            }
        }
        #endregion
    }
}
