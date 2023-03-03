namespace Enabill.Web
{
	public class TabPage
	{
		public TabPage(string title, string partialViewName = null, object model = null, string wrapperID = null, string body = null)
		{
			this.Title = title;
			this.Body = body;
			this.PartialViewName = partialViewName;
			this.Model = model;
			this.WrapperID = wrapperID;
		}

		public string Title { get; set; }
		public string Body { get; set; }
		public string PartialViewName { get; set; }
		public object Model { get; set; }
		public string WrapperID { get; set; }
	}
}