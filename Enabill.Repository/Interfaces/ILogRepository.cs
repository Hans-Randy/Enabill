using System.Collections.Generic;
using Enabill.Models.Dto;

namespace Enabill.Repository.Interfaces
{
	public interface ILogRepository : IBaseRepository
	{
		IList<LogItem> LogItems { get; set; }

		void GetLogEntries();
	}
}