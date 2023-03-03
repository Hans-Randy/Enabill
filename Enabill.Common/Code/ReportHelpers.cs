namespace Enabill.Code
{
	public static class ReportHelpers
	{
		public static string TemplatePath(string fileNameSuffix = "", string templatePath = "")
		{
			string result = "";

			if (string.IsNullOrEmpty(templatePath))
				result = Constants.PATHTEMPLATE.Replace("\\", "/");

			if (!templatePath.Contains(fileNameSuffix))
				return string.Concat(result, fileNameSuffix, ".xlsx");
			else
				return templatePath;
		}
	}
}