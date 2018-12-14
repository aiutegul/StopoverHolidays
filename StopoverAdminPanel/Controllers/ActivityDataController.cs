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
using StopoverAdminPanel.Models;

namespace StopoverAdminPanel.Controllers
{
	[Route("api/ActivityData/{action}", Name = "ActivityDataApi")]
	public class ActivityDataController : ApiController
	{
		private static List<ActivityData> activityDataList = new List<ActivityData>();
		private static int ActivityDataId = 0;
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			return Request.CreateResponse(DataSourceLoader.Load(activityDataList, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new ActivityData();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.Id = ActivityDataId;
			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			activityDataList.Add(model);
			ActivityDataId++;

			return Request.CreateResponse(HttpStatusCode.Created);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = activityDataList.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "Data not found");

			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);

			//Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpDelete]
		public void Delete(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = activityDataList.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			activityDataList.Remove(model);
		}

		[HttpPost]
		public HttpResponseMessage ProcessActivities(List<ActivityData> data)
		{
			var orderActivity = data.Select(o => new
			{
				o.OrderId,
				o.ActivityTimeId,
				o.ActivityId,
				o.ActivityDate,
				o.TransferLocation,
				o.TotalPrice
			}).First();

			var oA = new OrderActivity
			{
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow,
				DeletedDate = null,
				OrderId = orderActivity.OrderId,
				ActivityTimeId = orderActivity.ActivityTimeId,
				ActivityId = orderActivity.ActivityId,
				Date = orderActivity.ActivityDate,
				TransferLocation = orderActivity.TransferLocation,
				Price = orderActivity.TotalPrice,
				Comments = null
			};

			_context.OrderActivity.Add(oA);
			var passengerData = data.Select(p => new { p.FirstName, p.LastName, p.isChild });
			foreach (var passenger in passengerData)
			{
				var passengerToAdd = new Passenger
				{
					CreatedDate = DateTime.UtcNow,
					UpdatedDate = DateTime.UtcNow,
					DeletedDate = null,
					FirstName = passenger.FirstName,
					LastName = passenger.LastName,
					isChild = passenger.isChild,
					Comments = null
				};
				_context.Passenger.Add(passengerToAdd);

				var activityPassenger = new ActivityPassenger
				{
					CreatedDate = DateTime.UtcNow,
					UpdatedDate = DateTime.UtcNow,
					DeletedDate = null,
					OrderActivity = oA,
					Passenger = passengerToAdd,
					Comments = null
				};
				_context.ActivityPassenger.Add(activityPassenger);
			}

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, "Records Created");
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