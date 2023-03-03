using System.Configuration;
using System.Data;
using System.Data.OleDb;

namespace Alacrity.DataAccess.Excel
{
	public class DbManager : IDbManager
	{
		private IDbConnection _conn { get; set; }
		private string _connectionString;

		public IDbConnection Connection
		{
			get
			{
				if (this._conn.State == ConnectionState.Closed)
					this._conn.Open();

				return this._conn;
			}
		}

		public DbManager()
		{
		}

		public DbManager(string connectionStringName)
		{
			this._connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
			this._conn = new OleDbConnection(this._connectionString);
		}

		public void Dispose()
		{
			if (this._conn != null)
			{
				if (this._conn.State == ConnectionState.Open)
				{
					this._conn.Close();
					this._conn.Dispose();
				}
				this._conn = null;
			}
		}
	}
}