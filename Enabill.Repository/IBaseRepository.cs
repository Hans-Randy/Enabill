using Alacrity.DataAccess;

namespace Enabill.Repository
{
	public interface IBaseRepository
	{
		IDbManager DbManager { get; }
	}
}