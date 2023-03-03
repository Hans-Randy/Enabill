using System.Collections.Generic;
using System.Linq;
using Enabill.Models.Dto;
using Enabill.Repos;

namespace Enabill.Web.Models
{
	public class ForecastDefaultReferenceModel
	{
		#region INITIALIZATION

		public ForecastDefaultReferenceModel()
		{
			this.ForecastDefaultReferenceExtendedModel = ForecastReferenceDefaultRepo.GetAllDefaultReferenceExtendedModels().ToList();
		}

		#endregion INITIALIZATION

		#region PROPERTIES

		public List<ForecastDefaultReferenceExtendedModel> ForecastDefaultReferenceExtendedModel { get; private set; }

		#endregion PROPERTIES
	}
}