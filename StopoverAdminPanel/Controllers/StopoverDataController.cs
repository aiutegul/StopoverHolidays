using Amadeus.Api.Service.Client;
using Amadeus.PassengerNameRecords;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Newtonsoft.Json;
using StopoverAdminPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace StopoverAdminPanel.Controllers
{
    [Route("api/StopoverData/{action}", Name = "StopoverDataApi")]
    public class StopoverDataController : ApiController
    {
        private static List<StopoverData> stopoverDataList = new List<StopoverData>();
        private static Dictionary<string, IPNR> BookingRefPnrDict = new Dictionary<string, IPNR>();
        private static List<IPassenger> PnrPassengerList = new List<IPassenger>();
        private static int StopoverDataId = 0;
        private StopoverDbContext _context = new StopoverDbContext();

        [HttpGet]
        public HttpResponseMessage Get(DataSourceLoadOptions loadOptions)
        {
            return Request.CreateResponse(DataSourceLoader.Load(stopoverDataList, loadOptions));
        }

        [HttpPost]
        public HttpResponseMessage Post(FormDataCollection form)
        {
            var model = new StopoverData();
            var values = form.Get("values");
            JsonConvert.PopulateObject(values, model);
            model.Id = StopoverDataId;
            Validate(model);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            stopoverDataList.Add(model);
            StopoverDataId++;

            return Request.CreateResponse(HttpStatusCode.Created);
        }

        [HttpPut]
        public HttpResponseMessage Put(FormDataCollection form)
        {
            var key = Convert.ToInt32(form.Get("key"));
            var model = stopoverDataList.FirstOrDefault(item => item.Id == key);
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
            var model = stopoverDataList.FirstOrDefault(item => item.Id == key);

            if (model == null)
            {
                Request.CreateResponse(NotFound());
            }

            stopoverDataList.Remove(model);
        }

        

        [HttpPost]
        public HttpResponseMessage ProcessStopover(List<StopoverData> data)
        {
            //CompareAndAddPassengersFromPnr(bookingRefs, data);

            //Create an OrderStopover
            var orderStopover = data.Select(o => new
            {
                o.OrderId,
                o.HotelId,
                o.TransferId,
                o.IsPromo,
                o.CheckIn,
                o.CheckOut,
                o.DayUse,
                o.Nights,
                o.FromAirportTransferUsed,
                o.FromHotelTransferUsed,
                o.OrderStopoverPrice
            }).First();

            var os = new OrderStopover
            {
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                DeletedDate = null,
                OrderId = orderStopover.OrderId,
                HotelId = orderStopover.HotelId,
                TransferId = orderStopover.TransferId,
                IsPromo = orderStopover.IsPromo,
                CheckIn = orderStopover.CheckIn,
                CheckOut = orderStopover.CheckOut,
                DayUse = orderStopover.DayUse,
                Nights = orderStopover.Nights,
                FromAirportTransferUsed = orderStopover.FromAirportTransferUsed,
                FromHotelTransferUsed = orderStopover.FromHotelTransferUsed,
                Price = orderStopover.OrderStopoverPrice,
                Comments = null
            };

            Validate(os);
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

            _context.OrderStopover.Add(os);

            // Select flights from this massive amount of dump
            var flightsToAdd = data.Select(f => new
            {
                f.ArriveDate,
                f.DepartureDate,
                f.ArriveFlight,
                f.DepartureFlight,
                f.Routes,
                f.IsTransit,
                f.IsPointToPoint
            }).Distinct();

            // super-mega horrible 3 nested loops responsible for inserting new Flight, BookingReference,
            // Passenger and StopoverPassenger records
            foreach(var flightToAdd in flightsToAdd)
            {
                var flight = new Flight
                {
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow,
                    DeletedDate = null,
                    OrderStopover = os,
                    ArriveDate = flightToAdd.ArriveDate,
                    DepartureDate = flightToAdd.DepartureDate,
                    ArriveFlight = flightToAdd.ArriveFlight,
                    DepartureFlight = flightToAdd.DepartureFlight,
                    Routes = flightToAdd.Routes,
                    IsTransit = flightToAdd.IsTransit,
                    IsPointToPoint = flightToAdd.IsPointToPoint,
                    Comments = null
                };

                Validate(flight);

                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

                _context.Flight.Add(flight);


                var bookingRefsForFlights = data.Select(b => new
                {
                    b.BookingReference,
                    b.ArriveDate,
                    b.DepartureDate,
                    b.ArriveFlight,
                    b.DepartureFlight,
                    b.Routes,
                    b.IsTransit,
                    b.IsPointToPoint
                }).Where(b => b.ArriveDate == flight.ArriveDate && b.ArriveFlight == flight.ArriveFlight && 
                b.DepartureFlight == flight.DepartureFlight && b.DepartureDate == flight.DepartureDate 
                && b.Routes == flight.Routes && b.IsTransit == flight.IsTransit && b.IsPointToPoint == flight.IsPointToPoint);

                // booking refs for each flight
                var bookingRefsToAdd = bookingRefsForFlights.Select(b => b.BookingReference).Distinct();

                foreach(var bookingRefToAdd in bookingRefsToAdd)
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

                    Validate(bookingReference);
                    if (!ModelState.IsValid)
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

                    _context.BookingReference.Add(bookingReference);

                    var passengersInRoomsToAdd = data.Select(p => new
                    {
                        p.BookingReference,
                        p.FirstName,
                        p.LastName,
                        p.isChild,
                        p.RoomNum,
                        p.TicketNumber,
                        p.PromoUsed,
                        p.RoomTypeId,
                        p.HotelId
                    })
                        .Where(p => p.BookingReference == bookingRefToAdd);

                    foreach (var passenger in passengersInRoomsToAdd)
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

                        Validate(passengerToAdd);
                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

                        _context.Passenger.Add(passengerToAdd);

                        var room = _context.Room.Select(r => new { r.Id, r.HotelId, r.RoomTypeId })
                            .Where(r => r.HotelId == passenger.HotelId && r.RoomTypeId == passenger.RoomTypeId).First();

                        if (room == null)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

                        var stopoverPassenger = new StopoverPassenger
                        {
                            CreatedDate = DateTime.UtcNow,
                            UpdatedDate = DateTime.UtcNow,
                            DeletedDate = null,
                            OrderStopover = os,
                            Passenger = passengerToAdd,
                            RoomId = room.Id,
                            BookingReference = bookingReference,
                            RoomNum = passenger.RoomNum,
                            TicketNumber = passenger.TicketNumber,
                            PromoUsed = passenger.PromoUsed
                        };

                        Validate(stopoverPassenger);
                        if (!ModelState.IsValid)
                            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

                        _context.StopoverPassenger.Add(stopoverPassenger);
                    }
                }    

            }

            _context.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private void CompareAndAddPassengersFromPnr(IEnumerable<string> bookingRefs, List<StopoverData> data)
        {
            foreach (var bookingRef in bookingRefs)
            {
                IPNR pnr = GetPNR(bookingRef);
                BookingRefPnrDict.Add(bookingRef, pnr);
                //PnrPassengerList.AddRange(pnr.Passengers);
                var passengerList = data.Select(p => new { p.FirstName, p.LastName, p.isChild, p.BookingReference })
                    .Where(p => p.BookingReference == bookingRef);
                var pnrPassengerList = pnr.Passengers.Select(p => new { p.FirstName, p.LastName });
                var passengersToAdd = passengerList.Select(p => new {
                    p.FirstName,
                    p.LastName,
                    p.isChild
                })
                    .Where(p => pnrPassengerList.Any(i => i.FirstName == p.FirstName.ToUpper()
                    && i.LastName == p.LastName.ToUpper()));
                foreach (var passenger in passengersToAdd)
                {
                    _context.Passenger.Add(new Passenger
                    {
                        FirstName = passenger.FirstName,
                        LastName = passenger.LastName,
                        isChild = passenger.isChild
                    });
                    _context.SaveChanges();
                }
            }
        }

        private IPNR GetPNR(string bookingReference)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
