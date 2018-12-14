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
	[Route("api/FlightPassenger/{action}", Name = "FlightPassengersApi")]
	public class FlightPassengersController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var flightpassenger = _context.FlightPassenger.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.BookingReferenceId,
				i.FirstName,
				i.LastName,
				i.Type,
				i.TicketNumber
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(flightpassenger, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new FlightPassenger();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.FlightPassenger.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		public HttpResponseMessage PostPassengerFromPnr([FromBody]FlightPassenger flightPassenger)
		{
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
			var result = _context.FlightPassenger.Add(new FlightPassenger()
			{
				FirstName = flightPassenger.FirstName,
				LastName = flightPassenger.LastName,
				Type = flightPassenger.Type,
				BookingReferenceId = flightPassenger.BookingReferenceId,
				TicketNumber = flightPassenger.TicketNumber,
				CreatedDate = flightPassenger.CreatedDate,
				UpdatedDate = flightPassenger.UpdatedDate,
				DeletedDate = flightPassenger.DeletedDate
			});

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.FlightPassenger.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "FlightPassenger not found");

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
			var model = _context.FlightPassenger.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		public HttpResponseMessage BookingReferenceLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.BookingReference
						 orderby i.PNR
						 select new
						 {
							 Value = i.Id,
							 Text = i.PNR
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
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