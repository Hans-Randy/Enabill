using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Enabill.Properties;
using Enabill.Repos;

namespace Enabill.Models
{
	[Table("Regions")]
	public class Region
	{
		public Region()
		{
			this.IsNew = true;
		}
		public Region(string regionName, string regionShortCode, bool isActive)
		{
			this.RegionName = regionName;
			this.RegionShortCode = regionShortCode;
			this.IsActive = isActive;
		}
		#region PROPERTIES
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int RegionID { get; set; }
		[Required, MinLength(2), MaxLength(50)]
		public string RegionName { get; set; }
		[Required, MinLength(2), MaxLength(5)]
		public string RegionShortCode { get; set; }
		[Required]
		public bool IsActive { get; set; }
		[NotMapped]
		public bool IsNew { get; set; }
		#endregion PROPERTIES
		public static List<RegionSearchResult> Search(User userSearching, string q, bool isActive, bool showAllRegions = false)
		{
			if (userSearching.HasRole(UserRoleType.SystemAdministrator))
				return RegionRepo.Search(q, isActive, showAllRegions);

			throw new UserRoleException("You do not have the required permissions to retrieve the data");
		}
		public void Save(User userSaving)
		{
			if (userSaving.CanManage(this))
			{
				RegionRepo.Save(this);
				return;
			}
			if (this.RegionID > 0)
				throw new UserRoleException(Resources.ERR_NoPermissionToUpdateRegion);

			throw new UserRoleException(Resources.ERR_NoPermissionToUpdateRegion);
		}
		public void ValidateSave()
		{
			if (string.IsNullOrEmpty(this.RegionName))
				throw new EnabillException(Resources.ERR_NoRegionName_Message);

			if (string.IsNullOrEmpty(this.RegionShortCode))
				throw new EnabillException(Resources.ERR_NoRegionShortCode_Message);
		}

	}
	public class RegionSearchResult
	{
		public int RegionID { get; internal set; }
		public string RegionName { get; internal set; }
		public string RegionShortCode { get; internal set; }
		public bool IsActive { get; internal set; }
	}
	public class Status
	{
		public int StatusID { get; internal set; }
		public string StatusName { get; internal set; }
	}
}