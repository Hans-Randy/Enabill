using System.Web.Mvc;
using StoryWriter.Models;
using StoryWriter.Services;

namespace Enabill.Web.Controllers
{
	[Authorize]
	public class StoryController : BaseController
	{
		public ActionResult Index() => this.View(StoryService.Search(""));

		public ActionResult Board() => this.View(StoryService.GetAllForProject(EnabillSettings.StoryWriterProjectID));

		[HttpGet]
		public ActionResult Create()
		{
			var story = new Story();
			this.SetViewDropDownLists(story);

			return this.View("Edit", story);
		}

		[HttpGet]
		public ActionResult Page(string id)
		{
			var story = StoryService.GetByRef(id);

			if (story == null)
			{
				story = new Story
				{
					RefUrl = id
				};
			}

			this.SetViewDropDownLists(story);

			return this.View("Edit", story);
		}

		public ActionResult Details(int id)
		{
			var story = StoryService.GetByID(id);

			if (story == null)
				return this.HttpNotFound();

			return this.View(story);
		}

		public ActionResult Preview(string id)
		{
			var story = StoryService.GetByRef(id);

			if (story == null)
			{
				story = new Story
				{
					RefUrl = id
				};
			}

			return this.PartialView(story);
		}

		[HttpGet]
		public ActionResult Edit(int? id)
		{
			var story = StoryService.GetByID(id.Value);

			if (story == null)
				return new HttpNotFoundResult("Story not found");
			this.SetViewDropDownLists(story);

			return this.View(story);
		}

		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Edit(int? id, FormCollection form)
		{
			Story story;

			if (id != null)
			{
				story = StoryService.GetByID(id.Value);
			}
			else
			{
				story = new Story
				{
					ProjectID = EnabillSettings.StoryWriterProjectID
				};
			}

			return this.Save(story);
		}

		private ActionResult Save(Story story)
		{
			try
			{
				if (this.TryUpdateModel(story))
				{
					StoryService.Save(story);
					return this.RedirectToAction("Index");
				}
			}
			catch
			{
			}

			this.SetViewDropDownLists(story);

			return this.View(story);
		}

		private void SetViewDropDownLists(Story model)
		{
			this.AddAsDropdownSource(ActorService.GetAll(), "Actor", model.ActorID);
			this.AddAsDropdownSource(StoryGroupService.FetchAllForProject(EnabillSettings.StoryWriterProjectID), "StoryGroup", model.StoryGroupID);
		}
	}
}