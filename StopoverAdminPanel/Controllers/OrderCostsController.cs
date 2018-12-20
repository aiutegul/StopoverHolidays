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
	[Route("api/OrderCost/{action}", Name = "OrderCostsApi")]
	[Authorize(Roles = "Admin")]
	public class OrderCostsController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var ordercost = _context.OrderCost.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.OrderId,
				i.AirfareTaxes,
				i.FirstNightPackageCost,
				i.RestNightGross,
				i.ToursGross,
				i.TotalExpenseFirstNight,
				i.TotalExpenseTransfer,
				i.RestNightNet,
				i.TursNet,
				i.OneDollarExpense,
				i.SalesCommission,
				i.ORCCommission,
				i.Profit,
				i.KCRevenue,
				i.PercentFromTicketPrice,
				i.Content,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(ordercost, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new OrderCost();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.OrderCost.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.OrderCost.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "OrderCost not found");

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
			var model = _context.OrderCost.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		public HttpResponseMessage OrderLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Order
						 orderby i.RegistrationNumber
						 select new
						 {
							 Value = i.Id,
							 Text = i.RegistrationNumber
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage GetOrderCostForOrder(int orderId, DataSourceLoadOptions loadOptions)
		{
			var ordercost = _context.OrderCost.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.OrderId,
				i.AirfareTaxes,
				i.FirstNightPackageCost,
				i.RestNightGross,
				i.ToursGross,
				i.TotalExpenseFirstNight,
				i.TotalExpenseTransfer,
				i.RestNightNet,
				i.TursNet,
				i.OneDollarExpense,
				i.SalesCommission,
				i.ORCCommission,
				i.Profit,
				i.KCRevenue,
				i.PercentFromTicketPrice,
				i.Content,
				i.Comments
			}).Where(i => i.DeletedDate == null).Where(i => i.OrderId == orderId);
			return Request.CreateResponse(DataSourceLoader.Load(ordercost, loadOptions));
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