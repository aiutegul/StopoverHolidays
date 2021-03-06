﻿using System;
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
	[Route("api/BookingReference/{action}", Name = "BookingReferencesApi")]
	public class BookingReferencesController : ApiController
	{
		private StopoverDbContext _context = new StopoverDbContext();

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
		{
			var bookingreference = _context.BookingReference.Select(i => new
			{
				i.Id,
				i.CreatedDate,
				i.UpdatedDate,
				i.DeletedDate,
				i.FlightId,
				i.PNR,
				i.Comments
			}).Where(i => i.DeletedDate == null);
			return Request.CreateResponse(DataSourceLoader.Load(bookingreference, loadOptions));
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new BookingReference();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);
			model.CreatedDate = DateTime.UtcNow;
			model.UpdatedDate = DateTime.UtcNow;
			model.DeletedDate = null;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.BookingReference.Add(model);
			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		public HttpResponseMessage PostBookingReferenceFromPnr([FromBody]BookingReference bookingReference)
		{
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));
			var result = _context.BookingReference.Add(new BookingReference()
			{
				PNR = bookingReference.PNR,
				FlightId = bookingReference.FlightId,
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow,
				DeletedDate = null,
				Comments = bookingReference.Comments
			});

			_context.SaveChanges();

			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.BookingReference.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "BookingReference not found");

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
			var model = _context.BookingReference.FirstOrDefault(item => item.Id == key);

			if (model == null)
			{
				Request.CreateResponse(NotFound());
			}

			model.DeletedDate = DateTime.UtcNow;
			_context.SaveChanges();
		}

		[HttpGet]
		[Authorize(Roles = "Admin, Office")]
		public HttpResponseMessage FlightLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Flight
						 orderby i.ArriveFlight
						 select new
						 {
							 Value = i.Id,
							 Text = i.ArriveFlight
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