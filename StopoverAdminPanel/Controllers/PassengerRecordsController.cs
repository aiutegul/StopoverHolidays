using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Amadeus.Api.Service.Client;
using Amadeus.PassengerNameRecords;
using Newtonsoft.Json;
using RestSharp;
using StopoverAdminPanel.Models;

namespace StopoverAdminPanel.Controllers
{
	public class PassengerRecordsController : Controller
	{
		private static Dictionary<string, IPNR> BookingRefPnrDict = new Dictionary<string, IPNR>();

		// GET: FlightPassengers
		public ActionResult FlightInfo(string bookingRef)
		{
			IPNR pnr = GetFlight(bookingRef);
			if (pnr == null)
			{
				return HttpNotFound();
			}
			BookingRefPnrDict.Add(bookingRef, pnr);
			string route = ExtractRouteFromPnr(pnr);
			ViewBag.BookingReference = bookingRef;
			ViewBag.Route = route;
			return PartialView();
		}

		public ActionResult Index(int id)
		{
			ViewBag.OrderStopoverId = id;
			return View();
		}

		public void RemovePnr(string data)
		{
			dynamic obj = JsonConvert.DeserializeObject(data);
			List<string> bookingRefList = obj["key"].ToObject<List<string>>();
			foreach (var bookingReference in bookingRefList)
			{
				BookingRefPnrDict.Remove(bookingReference);
			}
		}

		public ActionResult StopoverData(int id)
		{
			ViewBag.OrderId = id;
			return View();
		}

		public ActionResult ActivityData(int id)
		{
			ViewBag.OrderId = id;
			return View();
		}

		[HttpPost]
		public ActionResult LoadPassengers(string data)
		{
			dynamic obj = JsonConvert.DeserializeObject(data);
			List<string> bookingRefList = obj["key"].ToObject<List<string>>();

			foreach (string bookingRef in bookingRefList)
			{
				//get Id's of the bookingRefs from context.BookingReference
				//to insert values into FlightPassenger table

				IPNR pnr = BookingRefPnrDict[bookingRef];
				//int flightId = AddFlight(pnr, osId);
				//AddBookingRef(bookingRef, flightId);
				AddFlightPassengers(pnr);
			}
			return PartialView();
		}

		[HttpPost]
		public ActionResult ValidatePNRs(string data, int osId)
		{
			dynamic obj = JsonConvert.DeserializeObject(data);
			List<string> bookingRefList = obj["key"].ToObject<List<string>>();

			foreach (string bookingRef in bookingRefList)
			{
				IPNR pnr = BookingRefPnrDict[bookingRef];
				int flightId = AddFlight(pnr, osId);
				if (flightId == -1)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Flight from booking reference: "
						 + bookingRef + " could not be added");
				}

				int bookingRefId = AddBookingRef(bookingRef, flightId);
				if (bookingRefId == -1)
				{
					return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Booking Reference " +
						bookingRef + " could not be added");
				}
			}

			return RedirectToAction("Index");
		}

		private int AddBookingRef(string bookingRef, int flightId)
		{
			BookingReference bookingReference = new BookingReference();
			bookingReference.FlightId = flightId;
			bookingReference.PNR = bookingRef;
			bookingReference.Comments = null;
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri("http://localhost:52561/");

				//HTTP POST
				var postTask = client.PostAsJsonAsync<BookingReference>("api/Flight/PostFlightFromPnr",
					bookingReference);
				postTask.Wait();

				var result = postTask.Result;
				if (result.IsSuccessStatusCode)
				{
					int bookingReferenceId = Convert.ToInt32(result.Content.ToString());
					return bookingReferenceId;
				}
			}

			ModelState.AddModelError(string.Empty, "Server Error. Please contact administrator.");

			return -1;
		}

		private void AddFlightPassengers(IPNR pnr)
		{
			foreach (var passenger in pnr.Passengers)
			{
				var flightPassenger = new FlightPassenger();
				flightPassenger.FirstName = passenger.FirstName;
				flightPassenger.LastName = passenger.LastName;
				flightPassenger.Type = passenger.Type.ToString();
				var ticketNo = findTicket(passenger.ID, pnr.Tickets);
				flightPassenger.TicketNumber = ticketNo;
				flightPassenger.BookingReferenceId = 1;
				flightPassenger.CreatedDate = DateTime.UtcNow;
				flightPassenger.UpdatedDate = DateTime.UtcNow;
				flightPassenger.DeletedDate = null;
				using (var client = new HttpClient())
				{
					client.BaseAddress = new Uri("http://localhost:52561/");

					//HTTP POST
					var postTask = client.PostAsJsonAsync<FlightPassenger>("api/FlightPassenger/PostPassengerFromPnr",
						flightPassenger);
					postTask.Wait();

					var result = postTask.Result;
					//if (!result.IsSuccessStatusCode)
					//{
					//    return BadRequest();
					//}
				}

				ModelState.AddModelError(string.Empty, "Server Error. Please contact administrator.");
			}
		}

		private int AddFlight(IPNR pnr, int orderStopoverId)
		{
			// Need to figure out how Arrival and departure dates are determined for a flight from itineraries
			// For now just take a random one to match existing records

			var flight = new Flight();
			flight.ArriveDate = pnr.Itineraries[0].ArrivalTime.GetValueOrDefault(DateTime.Now);// Need to consult
			flight.DepartureDate = pnr.Itineraries[1].DepartureTime.GetValueOrDefault(DateTime.Now);
			flight.IsPointToPoint = true;
			flight.IsTransit = true;
			flight.DepartureFlight = pnr.Itineraries[1].FullFlightNumber;
			flight.ArriveFlight = pnr.Itineraries[0].FullFlightNumber;
			flight.Routes = ExtractRouteFromPnr(pnr);
			flight.Comments = null;
			flight.OrderStopoverId = orderStopoverId;

			//using (var client = new HttpClient())
			//{
			//    client.BaseAddress = new Uri("http://localhost:52561/");

			//    //HTTP POST
			//    var postTask = client.PostAsJsonAsync<Flight>("api/Flight/PostFlightFromPnr",
			//        flight);
			//    postTask.Wait();

			//    var result = postTask.Result;
			//    if (result.IsSuccessStatusCode)
			//    {
			//        int flightId = Convert.ToInt32(result.Content.ToString());
			//        return flightId;
			//    }
			//}

			//ModelState.AddModelError(string.Empty, "Server Error. Please contact administrator.");
			var client = new RestClient("http://localhost:52561/");
			var request = new RestRequest("api/Flight/PostFlightFromPnr", Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(flight);
			var queryResult = client.Execute<int>(request).Data;

			return queryResult;
		}

		private string findTicket(int iD, IList<ITicket> tickets)
		{
			foreach (var ticket in tickets)
			{
				if (ticket.PassengersIDs.Contains(iD))
				{
					return ticket.Number;
				}
			}
			return null;
		}

		private string ExtractRouteFromPnr(IPNR pnr)
		{
			string route = "";
			foreach (var itinerary in pnr.Itineraries.ToList())
			{
				route += itinerary.DepartureAirportCode + "-";
			}
			route += pnr.Itineraries.ToList().Last().ArrivalAirportCode;

			return route;
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