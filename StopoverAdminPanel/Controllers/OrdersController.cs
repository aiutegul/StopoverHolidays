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
	[Route("api/Order/{action}", Name = "OrdersApi")]
	public class OrdersController : ApiController
	{
		private readonly StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var order = _context.Order.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.RegistrationNumber,
				i.CityId,
				i.Email,
				i.Referral,
				i.PartnerId,
				i.MailSent,
				i.Passengers,
				i.Language,
				i.Comments
			}).Where(i => i.DeletedDate == null);
		   
            return Request.CreateResponse(DataSourceLoader.Load(order, loadOptions));
		}

		[Authorize(Roles = "Admin")]
		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new Order();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.Order.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.Order.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "Order not found");

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
			var model = _context.Order.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		[Authorize(Roles = "Admin, Office, User")]
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
		[Authorize(Roles = "Admin, Office, User")]
		public HttpResponseMessage PartnerLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Partner
						 orderby i.Code
						 where i.DeletedDate == null
						 select new
						 {
							 Value = i.Id,
							 Text = i.Code
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

	    [HttpGet]
	    [Authorize(Roles = "Admin")]
	    public HttpResponseMessage FindOrders(string queryString)
	    {
	        var searchResults = _context.StopoverSearch_v.Where(s => s.findstring.Contains(queryString));
	        var orderIDs = searchResults.Select(o => o.orderid).Distinct().ToList();
	        if (!orderIDs.Any())
	            return Request.CreateResponse(HttpStatusCode.BadRequest, "Search Returned No results");
	       
	        //return Request.CreateResponse();
	        return Request.CreateResponse(HttpStatusCode.OK, orderIDs);
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