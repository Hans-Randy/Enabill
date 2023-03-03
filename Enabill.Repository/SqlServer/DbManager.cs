using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Alacrity.DataAccess.SqlServer
{
	public class DbManager : IDbManager
	{
		private IDbConnection Conn { get; set; }
		private string connectionString;

		public IDbConnection Connection
		{
			get
			{
				if (this.Conn.State == ConnectionState.Closed)
					this.Conn.Open();

				return this.Conn;
			}
		}

		public DbManager()
		{
		}

		public DbManager(string connectionStringName)
		{
			this.connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
			this.Conn = new SqlConnection(this.connectionString);
		}

		public void Dispose()
		{
			if (this.Conn != null)
			{
				if (this.Conn.State == ConnectionState.Open)
				{
					this.Conn.Close();
					this.Conn.Dispose();
				}
				this.Conn = null;
			}
		}
	}
}