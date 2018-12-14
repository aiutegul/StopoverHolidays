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
	[Route("api/HotelBlockedDay/{action}", Name = "HotelBlockedDaysApi")]
	public class HotelBlockedDaysController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var hotelblockedday = _context.HotelBlockedDay.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.StartDate,
				i.EndDate,
				i.HotelId,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(hotelblockedday, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new HotelBlockedDay();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.HotelBlockedDay.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.HotelBlockedDay.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "HotelBlockedDay not found");

			model.UpdatedDate = DateTime.UtcNow;
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);

			// Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpDelete]
		public void Delete(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.HotelBlockedDay.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		public HttpResponseMessage HotelLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Hotel
						 orderby i.NameCode
						 where i.DeletedDate == null
						 select new
						 {
							 Value = i.Id,
							 Text = i.NameCode
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage GetBlockedDaysForHotel(int hotelId, DataSourceLoadOptions loadOptions)
		{
			var hotelblockedday = _context.HotelBlockedDay.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.StartDate,
				i.EndDate,
				i.HotelId,
				i.Comments
			}).Where(i => i.DeletedDate == null).Where(i => i.HotelId == hotelId);
			return Request.CreateResponse(DataSourceLoader.Load(hotelblockedday, loadOptions));
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