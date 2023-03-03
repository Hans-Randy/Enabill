using System.Configuration;
using System.IO;
using System.Web.Hosting;

namespace Enabill.Web.Code
{
	public class DiskFileStore
	{
		private string _path;

		public string GetPathName => this._path ?? (this._path = Enabill.Code.Constants.PATHTEMP ?? HostingEnvironment.MapPath("~/App_Data/Files"));

		public string GetDiskLocation(string getPathName) => Path.Combine(getPathName);

		public string StorageFileName(string fileName)
		{
			if (File.Exists(this.GetDiskLocation(fileName)))
			{
				return fileName;
			}

			return fileName;
		}
	}
}