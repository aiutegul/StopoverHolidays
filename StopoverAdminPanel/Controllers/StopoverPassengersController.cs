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
	[Route("api/StopoverPassenger/{action}", Name = "StopoverPassengersApi")]
	public class StopoverPassengersController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var stopoverpassenger = _context.StopoverPassenger.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.RoomId,
				i.OrderStopoverId,
				i.PassengerId,
				i.BookingReferenceId,
				i.RoomNum,
				i.TicketNumber,
				i.PromoUsed,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(stopoverpassenger, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new StopoverPassenger();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.StopoverPassenger.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.StopoverPassenger.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "StopoverPassenger not found");

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
		public void Delete(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.StopoverPassenger.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		public HttpResponseMessage PassengerLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Passenger
						 orderby i.LastName
						 where i.DeletedDate == null
						 select new
						 {
							 Value = i.Id,
							 Text = i.FirstName + " " + i.LastName + (i.isChild ? " Child" : "")
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage RoomLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Room
						 orderby i.Id
						 select new
						 {
							 Value = i.Id,
							 Text = (from r in _context.RoomType
									 join t in _context.Translation on r.NameCode equals t.NameCode
									 where i.RoomTypeId == r.Id
									 where t.LangCode == "EN"
									 select t.Text)
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage BookingRefLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.BookingReference
						 orderby i.PNR
						 where i.DeletedDate == null
						 select new
						 {
							 Value = i.Id,
							 Text = i.PNR
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage GetStopoverPassengersForOrderStopover(int orderStopoverId, DataSourceLoadOptions loadOptions)
		{
			var stopoverpassenger = _context.StopoverPassenger.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.RoomId,
				i.OrderStopoverId,
				i.PassengerId,
				i.BookingReferenceId,
				i.RoomNum,
				i.TicketNumber,
				i.PromoUsed,
				i.Comments
			}).Where(i => i.DeletedDate == null).Where(i => i.OrderStopoverId == orderStopoverId);

			return Request.CreateResponse(DataSourceLoader.Load(stopoverpassenger, loadOptions));
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