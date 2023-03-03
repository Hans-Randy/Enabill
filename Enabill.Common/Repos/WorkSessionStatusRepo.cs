using System.Linq;
using Enabill.Models;

namespace Enabill.Repos
{
	public class WorkSessionStatusRepo : BaseRepo
	{
		#region WORKSESSION STATUS SPECIFIC

		public WorkSessionStatus GetByID(int worksessionStatusID) => DB.WorkSessionStatus
					.SingleOrDefault(i => i.WorkSessionStatusID == worksessionStatusID);

		#endregion WORKSESSION STATUS SPECIFIC
	}
}