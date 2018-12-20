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
	[Route("api/OrderStopoverData/{action}", Name = "OrderStopoverDataApi")]
	[Authorize(Roles = "Admin, User")]
	public class OrderStopoverDataController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var orderstopoverdata = _context.OrderStopoverData.Select(i => new
			{
				i.Id,
				i.OrderId,
				i.FirstName,
				i.LastName,
				i.IsChild,
				i.BookingReference,
				i.TicketNumber,
				i.RoomNum
			});
			return Request.CreateResponse(DataSourceLoader.Load(orderstopoverdata, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new OrderStopoverData();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.OrderStopoverData.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.OrderStopoverData.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "OrderStopoverData not found");

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
			var model = _context.OrderStopoverData.FirstOrDefault(item => item.Id == key);

			_context.OrderStopoverData.Remove(model);
			_context.SaveChanges();
		}

		[HttpGet]
		public HttpResponseMessage OrderRequestsLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.OrderRequests
						 orderby i.RequestDate
						 select new
						 {
							 Value = i.Id,
							 Text = i.RequestDate
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage GetOrderStopoverDataForRequest(int requestId, DataSourceLoadOptions loadOptions)
		{
			var orderstopoverdata = _context.OrderStopoverData.Select(i => new
			{
				i.Id,
				i.OrderId,
				i.FirstName,
				i.LastName,
				i.IsChild,
				i.BookingReference,
				i.TicketNumber,
				i.RoomTypeId,
				i.RoomNum
			}).Where(i => i.OrderId == requestId);
			return Request.CreateResponse(DataSourceLoader.Load(orderstopoverdata, loadOptions));
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