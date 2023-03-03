using System.Collections.Generic;
using System.Data;
using System.Linq;
using Alacrity.DataAccess;
using Dapper;
using Enabill.Models;
using Enabill.Repository.Interfaces;

namespace Enabill.Repository.SqlServer
{
	public class WorkAllocationRepository : BaseRepository, IWorkAllocationRepository
	{
		public WorkAllocationRepository()
		{
		}

		public WorkAllocationRepository(IDbManager dbManager)
			: base(dbManager)
		{
		}

		private IList<WorkAllocation> workAllocations;

		public IList<WorkAllocation> WorkAllocations => this.workAllocations;

		public void GetWorkAllocations() => this.workAllocations = this.DbManager.Connection.Query<WorkAllocation>("GetWorkAllocations", commandType: CommandType.StoredProcedure).ToList();
	}
}