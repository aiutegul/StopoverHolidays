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
    [Route("api/OrderRequests/{action}", Name = "OrderRequestsApi")]
    public class OrderRequestsController : ApiController
    {
        private StopoverDbContext _context = new StopoverDbContext();

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions) {
            var orderrequests = _context.OrderRequests.Select(i => new {
                i.Id,
                i.RequestDate,
                i.PartnerId,
                i.RequestType,
                i.RequestStatus
            });
            return Request.CreateResponse(DataSourceLoader.Load(orderrequests, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form) {
            var model = new OrderRequests();
            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.RequestStatus = 0;

            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            var result = _context.OrderRequests.Add(model);
            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, result.Id);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.OrderRequests.FirstOrDefault(item => item.Id == key);
            if(model == null)
                return Request.CreateResponse(HttpStatusCode.Conflict, "OrderRequests not found");

            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);

            //Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        public void Delete(FormDataCollection form) {
            var key = Convert.ToInt32(form.Get("key"));
            var model = _context.OrderRequests.FirstOrDefault(item => item.Id == key);
            
            // This should be a list really, depending on how they want to implement orders and orderstopovers
            var stopoverRequest = _context.OrderStopoverData.FirstOrDefault(item => item.OrderId == key);
            var activityRequest = _context.OrderActivityData.FirstOrDefault(item => item.OrderId == key);

            // I really don't like following if's, but oh well...
            if(stopoverRequest != null)
            {
                _context.OrderStopoverData.Remove(stopoverRequest);
            }
            
            if(activityRequest != null)
            {
                _context.OrderActivityData.Remove(activityRequest);
            }

            _context.OrderRequests.Remove(model);
            _context.SaveChanges();
        }

        [HttpPut]
        public HttpResponseMessage PutRequest([FromBody]int requestId)
        {
            _context.OrderRequests.FirstOrDefault(r => r.Id == requestId).RequestStatus = 1;
            _context.SaveChanges();
            return Request.CreateResponse(HttpStatusCode.OK, "Request Status Updated");
        }

        [HttpDelete]
        public void DeleteRequest([FromBody]int requestId)
        {
            var model = _context.OrderRequests.FirstOrDefault(item => item.Id == requestId);
            // This should be a list really, depending on how they want to implement orders and orderstopovers
            var stopoverRequest = _context.OrderStopoverData.FirstOrDefault(item => item.OrderId == requestId);
            var activityRequest = _context.OrderActivityData.FirstOrDefault(item => item.OrderId == requestId);

            if (stopoverRequest != null)
            {
                _context.OrderStopoverData.Remove(stopoverRequest);
            }

            if (activityRequest != null)
            {
                _context.OrderActivityData.Remove(activityRequest);
            }

            _context.OrderRequests.Remove(model);
            _context.SaveChanges();
        }

        [HttpPost]
        public HttpResponseMessage ConfirmRequest([FromBody]int requestId)
        {
            var orderRequest = _context.OrderRequests.FirstOrDefault(item => item.Id == requestId);
            //var stopoverData = _context.OrderStopoverData.FirstOrDefault(item => item.OrderId == requestId);
            var activityData = _context.OrderActivityData.Select(i => new
            {
                i.OrderId,
                i.FirstName,
                i.LastName,
                i.IsChild,
                i.ActivityId,
                i.ActivityDate,
                i.ActivityTimeId,
                i.CityId,
                i.TransferLocation
            })
            .Where(i => i.OrderId == requestId);

            switch (orderRequest.RequestType)
            {
                case 0:
                    createStopoverData(requestId);
                    break;
                case 1:
                    break;
                case 2:
                    break;
            }

            
            return Request.CreateResponse(HttpStatusCode.Created, "Records created!");
        }

        private void createStopoverData(int requestId)
        {
            var order = createStubOrder(requestId);
            _context.Order.Add(order);
            var orderStopover = createOrderStopover(requestId, order);
            order.RegistrationNumber = "SP" + orderStopover.Id;
            order.Email = "Sayat.Amanbayev@airastana.com";
            orderStopover.Order = order;
            _context.OrderStopover.Add(orderStopover);
            var flightData = _context.OrderStopoverData.Select(f => new
            {
                f.OrderId,
                f.ArriveDate,
                f.DepartureDate,
                f.ArriveFlight,
                f.DepartureFlight,
                f.Routes,
                f.IsTransit,
                f.IsPointToPoint
            }).Where(f => f.OrderId == requestId).Distinct();

            foreach(var flightDatum in flightData)
            {
                var flight = new Flight
                {
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeletedDate = null,
                    OrderStopover = orderStopover,
                    ArriveDate = flightDatum.ArriveDate.GetValueOrDefault(),
                    DepartureDate = flightDatum.DepartureDate.GetValueOrDefault(),
                    ArriveFlight = flightDatum.ArriveFlight,
                    DepartureFlight = flightDatum.DepartureFlight,
                    Routes = flightDatum.Routes,
                    IsTransit = flightDatum.IsTransit.GetValueOrDefault(),
                    IsPointToPoint = flightDatum.IsPointToPoint.GetValueOrDefault(),
                    Comments = null
                };
                
                _context.Flight.Add(flight);
                createBookingReference(requestId, flight, orderStopover);


            }
        }

        private Order createStubOrder(int requestId)
        {
            var orderRequest = _context.OrderRequests.FirstOrDefault(o => o.Id == requestId);
            var order = new Order
            {
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                DeletedDate = null,
                CityId = 1,
                PartnerId = orderRequest.PartnerId.GetValueOrDefault(),
                Email = "",
                Referral = "",
                Passengers = 0,
                Language = "EN",
                MailSent = false,
                Comments = null
            };
            return order;
        }

        private void createStopoverPassenger(int requestId, BookingReference bookingReference, OrderStopover orderStopover)
        {
            var passengersInRoomsToAdd = _context.OrderStopoverData.Select(p => new
            {
                p.BookingReference,
                p.FirstName,
                p.LastName,
                p.IsChild,
                p.RoomNum,
                p.TicketNumber,
                p.PromoUsed,
                p.RoomTypeId,
                p.HotelId
            }).Where(p => p.BookingReference == bookingReference.PNR);

            foreach (var passenger in passengersInRoomsToAdd)
            {
                var passengerToAdd = new Passenger
                {
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeletedDate = null,
                    FirstName = passenger.FirstName,
                    LastName = passenger.LastName,
                    isChild = passenger.IsChild.GetValueOrDefault(),
                    Comments = null
                };

                _context.Passenger.Add(passengerToAdd);

                var room = _context.Room.Select(r => new { r.Id, r.HotelId, r.RoomTypeId })
                    .Where(r => r.HotelId == passenger.HotelId && r.RoomTypeId == passenger.RoomTypeId).First();

                var stopoverPassenger = new StopoverPassenger
                {
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeletedDate = null,
                    OrderStopover = orderStopover,
                    Passenger = passengerToAdd,
                    RoomId = room.Id,
                    BookingReference = bookingReference,
                    RoomNum = passenger.RoomNum.GetValueOrDefault(),
                    TicketNumber = passenger.TicketNumber,
                    PromoUsed = passenger.PromoUsed
                };


                _context.StopoverPassenger.Add(stopoverPassenger);
            }
        }

        private void createBookingReference(int requestId, Flight flight, OrderStopover orderStopover)
        {
            var bookingRefsForFlights = _context.OrderStopoverData.Select(b => new
            {
                b.OrderId,
                b.BookingReference,
                b.ArriveDate,
                b.DepartureDate,
                b.ArriveFlight,
                b.DepartureFlight,
                b.Routes,
                b.IsTransit,
                b.IsPointToPoint
            }).Where(b => b.OrderId == requestId).Where(b => b.ArriveDate == flight.ArriveDate && b.ArriveFlight == flight.ArriveFlight &&
            b.DepartureFlight == flight.DepartureFlight && b.DepartureDate == flight.DepartureDate
            && b.Routes == flight.Routes && b.IsTransit == flight.IsTransit && b.IsPointToPoint == flight.IsPointToPoint);

            // booking refs for each flight
            var bookingRefsToAdd = bookingRefsForFlights.Select(b => b.BookingReference).Distinct();
            foreach (var bookingRefToAdd in bookingRefsToAdd)
            {
                var bookingReference = new BookingReference
                {
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeletedDate = null,
                    Flight = flight,
                    PNR = bookingRefToAdd,
                    Comments = null
                };

                _context.BookingReference.Add(bookingReference);
                createStopoverPassenger(requestId, bookingReference, orderStopover);
            }
        }


        private OrderStopover createOrderStopover(int requestId, Order order)
        {
            var orderStopoverData = _context.OrderStopoverData.Select(o => new
            {
                o.CityId,
                o.OrderId,
                o.HotelId,
                o.TransferId,
                o.FromAirportTransferUsed,
                o.FromHotelTransferUsed,
                o.IsPromo,
                o.CheckIn,
                o.CheckOut,
                o.DayUse,
                o.OrderStopoverPrice
            }).FirstOrDefault(o => o.OrderId == requestId);

            order.CityId = orderStopoverData.CityId.GetValueOrDefault();

            var span = orderStopoverData.CheckOut - orderStopoverData.CheckIn;

            var orderStopover = new OrderStopover
            {
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                DeletedDate = null,
                Order = order,
                HotelId = orderStopoverData.HotelId.GetValueOrDefault(),
                TransferId = orderStopoverData.TransferId.GetValueOrDefault(),
                FromAirportTransferUsed = orderStopoverData.FromAirportTransferUsed.GetValueOrDefault(),
                FromHotelTransferUsed = orderStopoverData.FromHotelTransferUsed.GetValueOrDefault(),
                IsPromo = orderStopoverData.IsPromo.GetValueOrDefault(),
                CheckIn = orderStopoverData.CheckIn.GetValueOrDefault(),
                CheckOut = orderStopoverData.CheckOut.GetValueOrDefault(),
                DayUse = orderStopoverData.DayUse.GetValueOrDefault(),
                Nights = (int)span.GetValueOrDefault().TotalDays,
                Price = orderStopoverData.OrderStopoverPrice.GetValueOrDefault(),
                Comments = null
            };

            return orderStopover;
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