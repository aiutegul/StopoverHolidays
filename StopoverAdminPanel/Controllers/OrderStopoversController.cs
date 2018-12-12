using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace StopoverAdminPanel.Models.Controllers
{
    [Route("api/OrderStopover/{action}", Name = "OrderStopoversApi")]
    public class OrderStopoversController : ApiController
    {
        private StopoverDbContext _context = new StopoverDbContext();

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var orderstopover = _context.OrderStopover.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.OrderId,
                i.TransferId,
                i.FromAirportTransferUsed,
                i.FromHotelTransferUsed,
                i.HotelId,
                i.IsPromo,
                i.CheckIn,
                i.CheckOut,
                i.DayUse,
                i.Nights,
                i.Price,
                i.Comments
            }).Where(i => i.DeletedDate == null);
            return Request.CreateResponse(DataSourceLoader.Load(orderstopover, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new OrderStopover();
            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.CreatedDate = DateTime.UtcNow;
            model.UpdatedDate = DateTime.UtcNow;
            model.DeletedDate = null;

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            if(model.CheckOut < model.CheckIn)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Checkout date cannot be earlier than " +
                    "Checkin date");
            }

            if(CheckInBlocked(model.CheckIn, model.HotelId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Hotel Blocked for Checkin date. " +
                    "Please select a different hotel or Checkin Date");
            }

            if(CheckInPromoDisabled(model.CheckIn, model.HotelId))
            {
                model.IsPromo = false;
            }

            var result = _context.OrderStopover.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.Id);
        }

        private bool CheckInPromoDisabled(DateTime checkIn, int hotelId)
        {
            var hotelpromodisable = _context.HotelPromoDisable.Select(i => new
            {
                i.HotelId,
                i.StartDate,
                i.EndDate
            }).Where(i => i.HotelId == hotelId).Where(i => checkIn >= i.StartDate && checkIn <= i.EndDate);
            return hotelpromodisable.Any();
        }

        private bool CheckInBlocked(DateTime checkIn, int hotelId)
        {
            var hotelblockedday = _context.HotelBlockedDay.Select(i => new
            {
                i.HotelId,
                i.StartDate,
                i.EndDate
            }).Where(i => i.HotelId == hotelId).Where(i => checkIn >= i.StartDate && checkIn <= i.EndDate);

            return hotelblockedday.Any();
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.OrderStopover.FirstOrDefault(item => item.Id == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "OrderStopover not found");

            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.UpdatedDate = DateTime.UtcNow;

            //Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            if (model.CheckOut <= model.CheckIn)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Checkout date cannot be earlier than " +
                    "Checkin date");
            }

            if (CheckInBlocked(model.CheckIn, model.HotelId))
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Hotel Blocked for check-in date. " +
                    "Please select a different hotel or check-in Date");
            }

            if (CheckInPromoDisabled(model.CheckIn, model.HotelId))
            {
                model.IsPromo = false;
            }

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.OrderStopover.FirstOrDefault(item => item.Id == key);

            if (model == null)
            {
                Request.CreateResponse(NotFound());
            }

            model.DeletedDate = DateTime.UtcNow;
            _context.SaveChanges();
        }

        [HttpGet]
        public HttpResponseMessage TransferLookup(DataSourceLoadOptions loadOptions)
        {
            var lookup = from i in _context.Transfer
                         orderby i.Id
                         where i.DeletedDate == null
                         select new
                         {
                             Value = i.Id,
                             Text = i.Id
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }


        [HttpGet]
        public HttpResponseMessage HotelLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Hotel
                         orderby i.NameCode
                         where i.DeletedDate == null
                         select new {
                             Value = i.Id,
                             Text = (from t in _context.Translation
                                     where i.NameCode == t.NameCode && t.DeletedDate == null && t.LangCode == "EN"
                                     select t.Text)
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage OrderLookup(DataSourceLoadOptions loadOptions) {
            var lookup = from i in _context.Order
                         orderby i.RegistrationNumber
                         where i.DeletedDate == null
                         select new {
                             Value = i.Id,
                             Text = i.RegistrationNumber
                         };
            return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
        }

        [HttpGet]
        public HttpResponseMessage GetOrderStopoverForOrder(int orderId, DataSourceLoadOptions loadOptions)
        {
            var orderstopover = _context.OrderStopover.Select(i => new {
                i.Id,
                i.CreatedDate,
                i.UpdatedDate,
                i.DeletedDate,
                i.OrderId,
                i.TransferId,
                i.FromAirportTransferUsed,
                i.FromHotelTransferUsed,
                i.HotelId,
                i.IsPromo,
                i.CheckIn,
                i.CheckOut,
                i.DayUse,
                i.Nights,
                i.Price,
                i.Comments
            }).Where(i => i.DeletedDate == null).Where(i => i.OrderId == orderId);
            return Request.CreateResponse(DataSourceLoader.Load(orderstopover, loadOptions));
        }

        private string GetFullErrorMessage(ModelStateDictionary modelState) {
            var messages = new List<string>();

            foreach(var entry in modelState) {
                foreach(var error in entry.Value.Errors)
                    messages.Add(error.ErrorMessage);
            }

            return String.Join(" ", messages);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}