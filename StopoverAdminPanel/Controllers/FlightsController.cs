using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Amadeus.Api.Service.Client;
using Amadeus.PassengerNameRecords;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;

namespace StopoverAdminPanel.Models.Controllers
{
	[Route("api/Flight/{action}", Name = "FlightsApi")]
	public class FlightsController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var flight = _context.Flight.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.OrderStopoverId,
				i.ArriveDate,
				i.DepartureDate,
				i.ArriveFlight,
				i.DepartureFlight,
				i.Routes,
				i.IsTransit,
				i.IsPointToPoint,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(flight, loadOptions));
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new Flight();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.Flight.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		public HttpResponseMessage PostFlightFromPnr([FromBody]Flight flight)
		{
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
			var result = _context.Flight.Add(new Flight()
			{
				ArriveDate = flight.ArriveDate,
				DepartureDate = flight.DepartureDate,
				OrderStopoverId = flight.OrderStopoverId,
				ArriveFlight = flight.ArriveFlight,
				DepartureFlight = flight.DepartureFlight,
				Routes = flight.Routes,
				IsTransit = flight.IsTransit,
				IsPointToPoint = flight.IsPointToPoint,
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow,
				DeletedDate = null,
				Comments = flight.Comments
			});

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.Flight.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "Flight not found");

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
			var model = _context.Flight.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage OrderStopoverLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.OrderStopover
						 orderby i.CheckIn
						 where i.DeletedDate == null
						 select new
						 {
							 Value = i.Id,
							 Text = i.CheckIn
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage GetFlightsForOrderStopover(int orderStopoverId, DataSourceLoadOptions loadOptions)
		{
			var flight = _context.Flight.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.OrderStopoverId,
				i.ArriveDate,
				i.DepartureDate,
				i.ArriveFlight,
				i.DepartureFlight,
				i.Routes,
				i.IsTransit,
				i.IsPointToPoint,
				i.Comments
			}).Where(i => i.DeletedDate == null).Where(i => i.OrderStopoverId == orderStopoverId);
			return Request.CreateResponse(DataSourceLoader.Load(flight, loadOptions));
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

		private IPNR GetFlight(string bookingReference)
		{
			IPNR pnr = null;

			using (var amadeusWorker = new AmadeusWorkerApiServiceClient())
			{
				try
				{
					pnr = amadeusWorker.Booking.RetrievePNR(bookingReference);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					// exception
				}
			}

			return pnr;
		}
	}
}