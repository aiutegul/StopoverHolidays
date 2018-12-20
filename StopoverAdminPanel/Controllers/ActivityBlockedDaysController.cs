using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;

namespace StopoverAdminPanel.Models.Controllers
{
	[Route("api/ActivityBlockedDay/{action}", Name = "ActivityBlockedDaysApi")]
	[Authorize(Roles = "Admin")]
	public class ActivityBlockedDaysController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var activityblockedday = _context.ActivityBlockedDay.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.StartDate,
				i.EndDate,
				i.ActivityId,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(activityblockedday, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new ActivityBlockedDay();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.ActivityBlockedDay.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.ActivityBlockedDay.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "ActivityBlockedDay not found");

			model.UpdatedDate = DateTime.UtcNow;
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);

			//Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpDelete]
		public void Delete(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.ActivityBlockedDay.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		public HttpResponseMessage ActivityLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Activity
						 orderby i.NameCode
						 select new
						 {
							 Value = i.Id,
							 Text = i.NameCode
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage GetBlockedDaysForActivity(int activityId, DataSourceLoadOptions loadOptions)
		{
			var activityblockedday = _context.ActivityBlockedDay.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.StartDate,
				i.EndDate,
				i.ActivityId,
				i.Comments
			}).Where(i => i.DeletedDate == null).Where(i => i.ActivityId == activityId);
			return Request.CreateResponse(DataSourceLoader.Load(activityblockedday, loadOptions));
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_context.Dispose();
			}
			base.Dispose(disposing);
		}

		private string GetFullErrorMessage(ModelStateDictionary modelState)
		{
			var messages = new List<string>();

			foreach (var entry in modelState)
			{
				foreach (var error in entry.Value.Errors)
					messages.Add(error.ErrorMessage);
			}

			return String.Join(" ", messages);
		}
	}
}