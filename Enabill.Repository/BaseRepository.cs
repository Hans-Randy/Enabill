using Alacrity.DataAccess;

namespace Enabill.Repository
{
	public class BaseRepository : IBaseRepository
	{
		private IDbManager dbManager;

		public IDbManager DbManager => this.dbManager;

		public BaseRepository()
		{
		}

		public BaseRepository(IDbManager dbManager)
		{
			this.dbManager = dbManager;
		}
	}
}