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
	[Route("api/Activity/{action}", Name = "ActivitiesApi")]
	public class ActivitiesController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		[Authorize(Roles = "Admin, User")]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var activity = _context.Activity.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.NameCode,
				i.CityId,
				i.Email,
				i.Enabled,
				i.TransferEnabled,
				i.MinPassengers,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(activity, loadOptions));
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new Activity();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.Activity.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.Activity.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "Activity not found");

			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.UpdatedDate = DateTime.UtcNow;

			//Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpDelete]
		[Authorize(Roles = "Admin")]
		public void Delete(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.Activity.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		[Authorize(Roles = "Admin, User")]
		public HttpResponseMessage CityLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.City
						 orderby i.IataCode
						 where i.DeletedDate == null
						 select new
						 {
							 Value = i.Id,
							 Text = i.IataCode
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage GetActivitiesForOrderActivity(int activityId, DataSourceLoadOptions loadOptions)
		{
			var activity = _context.Activity.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.NameCode,
				i.CityId,
				i.Email,
				i.Enabled,
				i.TransferEnabled,
				i.MinPassengers,
				i.Comments
			}).Where(i => i.DeletedDate == null).Where(i => i.Id == activityId);
			return Request.CreateResponse(DataSourceLoader.Load(activity, loadOptions));
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