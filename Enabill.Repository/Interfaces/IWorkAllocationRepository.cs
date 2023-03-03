using System.Collections.Generic;
using Enabill.Models;

namespace Enabill.Repository.Interfaces
{
	public interface IWorkAllocationRepository : IBaseRepository
	{
		IList<WorkAllocation> WorkAllocations { get; }

		void GetWorkAllocations();
	}
}