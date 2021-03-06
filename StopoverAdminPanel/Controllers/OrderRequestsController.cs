﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using StopoverAdminPanel.Auth;
using StopoverAdminPanel.CurrencyConverterServiceReference;

namespace StopoverAdminPanel.Models.Controllers
{
	[Route("api/OrderRequests/{action}", Name = "OrderRequestsApi")]
	[Authorize(Roles = "Admin, User")]
	public class OrderRequestsController : ApiController
	{
		private readonly AuthRepository _repo = new AuthRepository();

		private StopoverDbContext _context = new StopoverDbContext();

		private List<RequestType> requestTypes = new List<RequestType>
		{
			new RequestType
			{
				Id = 0,
				Name = "Stopover"
			},
			new RequestType
			{
				Id = 1,
				Name = "Activity"
			},
			new RequestType
			{
				Id = 2,
				Name = "Both"
			}
		};

		private List<RequestStatus> requestStatuses = new List<RequestStatus>
		{
			new RequestStatus
			{
				StatusId = 0,
				Name = "New"
			},
			new RequestStatus
			{
				StatusId = 1,
				Name = "Submitted"
			},
			new RequestStatus
			{
				StatusId = 2,
				Name = "Confirmed"
			},
			new RequestStatus
			{
				StatusId = 3,
				Name = "Rejected"
			}
		};

		[HttpGet]
		public HttpResponseMessage RequestTypeLookup(DataSourceLoadOptions loadOptions)
		{
			return Request.CreateResponse(DataSourceLoader.Load(requestTypes, loadOptions));
		}

		[HttpGet]
		public HttpResponseMessage RequestStatusLookup(DataSourceLoadOptions loadOptions)
		{
			return Request.CreateResponse(DataSourceLoader.Load(requestStatuses, loadOptions));
		}

		[HttpGet]
		public async Task<HttpResponseMessage> Get(DataSourceLoadOptions loadOptions)
		{
			var isUser = HttpContext.Current.User.IsInRole("User");
		    string userId = User.Identity.GetUserId();
		    var user = await _repo.FindUserById(userId);
		    var partnerId = user.PartnerId;
           
            if (isUser)
			{
				
				var orderrequests = _context.OrderRequests.Select(i => new
				{
					i.Id,
					i.RequestDate,
					i.PartnerId,
					i.RequestType,
					i.RequestStatus,
					i.RegistrationNumber,
					i.CityId,
					i.HotelId,
					i.NumberOfPassengers,
					i.CheckIn,
					i.CheckOut,
					i.FromAirportTransferUsed,
					i.FromHotelTransferUsed,
					i.DayUse,
					i.Nights,
					i.ArriveDate,
					i.DepartureDate,
					i.ArriveFlight,
					i.DepartureFlight,
					i.Routes,
					i.Comments
				}).Where(p => p.PartnerId == partnerId);
				return Request.CreateResponse(DataSourceLoader.Load(orderrequests, loadOptions));
			}

			var orderRequests = _context.OrderRequests.Select(i => new
			{
				i.Id,
				i.RequestDate,
				i.PartnerId,
				i.RequestType,
				i.RequestStatus,
				i.RegistrationNumber,
				i.CityId,
				i.HotelId,
				i.NumberOfPassengers,
				i.CheckIn,
				i.CheckOut,
				i.FromAirportTransferUsed,
				i.FromHotelTransferUsed,
				i.DayUse,
				i.Nights,
				i.ArriveDate,
				i.DepartureDate,
				i.ArriveFlight,
				i.DepartureFlight,
				i.Routes,
				i.Comments
			});
			return Request.CreateResponse(DataSourceLoader.Load(orderRequests, loadOptions));
		}

		[HttpPost]
		public HttpResponseMessage Post(FormDataCollection form)
		{
			var model = new OrderRequests();
			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);

			model.RequestDate = DateTime.UtcNow;
			model.RequestStatus = 0;

			if (model.CheckOut < model.CheckIn)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Checkout date cannot be earlier than " +
					"Checkin date");
			}

			if (CheckInBlocked(model.CheckIn.GetValueOrDefault(), model.HotelId.GetValueOrDefault()))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Hotel Blocked for Checkin date. " +
					"Please select a different hotel or Checkin Date");
			}

			if (model.DayUse.GetValueOrDefault())
			{
				TimeSpan checkInTime = new TimeSpan(7, 0, 0);
				TimeSpan checkOutTime = new TimeSpan(19, 0, 0);
				model.CheckIn = model.CheckIn.GetValueOrDefault().Date + checkInTime;
				model.CheckOut = model.CheckOut.GetValueOrDefault().Date + checkOutTime;
				model.Nights = 1;
			}

			var span = model.CheckOut - model.CheckIn;
			model.Nights = span.GetValueOrDefault().Days;
			model.PartnerId = 2;

			Validate(model);
			if (!ModelState.IsValid)
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, GetFullErrorMessage(ModelState));

			var result = _context.OrderRequests.Add(model);
			_context.SaveChanges();

			//model.RegistrationNumber = "SP" + result.Id; // should be a switch from model.type
			//_context.SaveChanges();
			return Request.CreateResponse(HttpStatusCode.Created, result.Id);
		}

		[HttpPut]
		public HttpResponseMessage Put(FormDataCollection form)
		{
			var key = Convert.ToInt32(form.Get("key"));
			var model = _context.OrderRequests.FirstOrDefault(item => item.Id == key);
			if (model == null)
				return Request.CreateResponse(HttpStatusCode.Conflict, "OrderRequests not found");

			var values = form.Get("values");
			JsonConvert.PopulateObject(values, model);

		    if (model.RequestStatus == 2)
		    {
		        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cannot edit confirmed order" +
		                                                                      " request!");
            }

			if (model.CheckOut < model.CheckIn)
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Checkout date cannot be earlier than " +
					"Checkin date");
			}

			if (CheckInBlocked(model.CheckIn.GetValueOrDefault(), model.HotelId.GetValueOrDefault()))
			{
				return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Hotel Blocked for Checkin date. " +
					"Please select a different hotel or Checkin Date");
			}

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
			deleteEntireRequest(key);
		}

		[HttpGet]
		public HttpResponseMessage HotelLookup(DataSourceLoadOptions loadOptions)
		{
			var lookup = from i in _context.Hotel
						 orderby i.NameCode
						 where i.DeletedDate == null
						 select new
						 {
							 CityId = i.CityId,
							 Value = i.Id,
							 Text = (from t in _context.Translation
									 where i.NameCode == t.NameCode && t.DeletedDate == null && t.LangCode == "EN"
									 select t.Text)
						 };
			return Request.CreateResponse(DataSourceLoader.Load(lookup, loadOptions));
		}

		[HttpPut]
		public HttpResponseMessage SubmitRequest(int id)
		{
			var orderRequest = _context.OrderRequests.First(i => i.Id == id);
			switch (orderRequest.RequestType)
			{
				case 0:
					if (!_context.OrderStopoverData.Any(i => i.OrderId == id))
					{
						return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Stopover Data provided!");
					}
					orderRequest.RequestStatus = 1;
					break;

				case 1:
					if (!_context.OrderActivityData.Any(i => i.OrderId == id))
					{
						return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Activity Data provided!");
					}
					orderRequest.RequestStatus = 1;
					break;

				case 2:
					if (!_context.OrderActivityData.Any(i => i.OrderId == id) || !_context.OrderStopoverData.Any(i => i.OrderId == id))
					{
						return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No Activity or Stopover Data provided!");
					}
					orderRequest.RequestStatus = 1;
					break;
			}
			_context.SaveChanges();
			return Request.CreateResponse(HttpStatusCode.OK, "Request Status Updated");
		}

		[HttpPut]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage RejectRequest(int id, string reject_message)
		{
			var model = _context.OrderRequests.FirstOrDefault(r => r.Id == id);
			model.RequestStatus = 3;
			model.Comments = reject_message;
			_context.SaveChanges();
			return Request.CreateResponse(HttpStatusCode.OK);
		}

		[HttpPost]
		[Authorize(Roles = "Admin")]
		public HttpResponseMessage ConfirmRequest(int requestId)
		{
			var orderRequest = _context.OrderRequests.FirstOrDefault(item => item.Id == requestId);
			HttpResponseMessage confirmation_message = null;
			string message = "";

			switch (orderRequest.RequestType)
			{
				case 0:
					message = createStopoverData(orderRequest);
					if (message != "Records Created!")
					{
						confirmation_message = Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
					}
					else
					{
						confirmation_message = Request.CreateResponse(HttpStatusCode.Created, message);
					}
					break;

				case 1:
					createActivityData(requestId);
					break;

				default:
					message = createStopoverData(orderRequest);
					if (message != "Records Created!")
					{
						confirmation_message = Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
					}
					else
					{
						confirmation_message = Request.CreateResponse(HttpStatusCode.Created, message);
					}
					createActivityData(requestId);
					break;
			}

			return confirmation_message;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_context.Dispose();
			}
			base.Dispose(disposing);
		}

		private void deleteEntireRequest(int requestId)
		{
			var model = _context.OrderRequests.FirstOrDefault(item => item.Id == requestId);
			// This should be a list really, depending on how they want to implement orders and orderstopovers
			var stopoverRequests = _context.OrderStopoverData.Where(item => item.OrderId == requestId);
			var activityRequests = _context.OrderActivityData.Where(item => item.OrderId == requestId);

			if (stopoverRequests.Any())
			{
				foreach (var stopoverRequest in stopoverRequests)
				{
					_context.OrderStopoverData.Remove(stopoverRequest);
				}
			}

			if (activityRequests.Any())
			{
				foreach (var activityRequest in activityRequests)
				{
					_context.OrderActivityData.Remove(activityRequest);
				}
			}

			_context.OrderRequests.Remove(model);
			_context.SaveChanges();
		}

		private void createActivityData(int requestId)
		{
			throw new NotImplementedException();
		}

		private string createStopoverData(OrderRequests orderRequest)
		{
			var order = createStubOrder(orderRequest);
			_context.Order.Add(order);
			var orderStopover = createOrderStopover(orderRequest, order);

			var routes = orderRequest.Routes.Split();

			//Need logic over here to check if flight is transit or it's point to point
			//Then update OrderStopover by checking if Promo is available for it (setting isPromo to true or false)
			var flight = new Flight
			{
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow,
				DeletedDate = null,
				OrderStopover = orderStopover,
				ArriveDate = orderRequest.ArriveDate.GetValueOrDefault(),
				DepartureDate = orderRequest.DepartureDate.GetValueOrDefault(),
				ArriveFlight = orderRequest.ArriveFlight,
				DepartureFlight = orderRequest.DepartureFlight,
				Routes = orderRequest.Routes,
				IsPointToPoint = checkFlightIsPointToPoint(routes),
				IsTransit = checkFlightIsTransit(routes),
				Comments = null
			};

			orderStopover.IsPromo = checkStopoverPromo(orderStopover, flight);
		    orderStopover.Price = calculateStopoverPrice(orderRequest.Id, orderStopover);

            _context.Flight.Add(flight);
		    

            createBookingReference(orderRequest.Id, flight, orderStopover);

			try
			{
				_context.SaveChanges();
			}
			catch (Exception ex)
			{
				return ex.InnerException.Message;
			}

			order.RegistrationNumber = "SP" + order.Id;
			order.Email = "Sayat.Amanbayev@airastana.com";
			order.Referral = "Agent";

			//orderStopover.Price = calculateStopoverPrice(orderRequest.Id, orderStopover);

			orderRequest.RegistrationNumber = order.RegistrationNumber; // makes more sense this way
			orderRequest.RequestStatus = 2; //confirmed

			try
			{
				_context.SaveChanges();
			}
			catch (Exception ex)
			{
				return ex.InnerException.Message;
			}

			return "Records Created!";
		}

		private bool checkStopoverPromo(OrderStopover orderStopover, Flight flight)
		{
			var hotel = _context.Hotel.Find(orderStopover.HotelId);

			return !CheckInPromoDisabled(orderStopover.CheckIn, hotel.Id) &&
				   (hotel.IsPromo &&
					((flight.IsPointToPoint && orderStopover.Nights > 1)
					 || flight.IsTransit));
		}

		private bool checkFlightIsPointToPoint(string[] routes)
		{
			using (StopoverHolidaysServiceReference.StopoverHolidaysServiceClient client =
				new StopoverHolidaysServiceReference.StopoverHolidaysServiceClient())
			{
				//CHANGE THIS TO USE SERVICE.ISPOINTTOPOINT
				return client.IsPointToPoint(routes);
			}
		}

		private bool checkFlightIsTransit(string[] routes)
		{
			using (StopoverHolidaysServiceReference.StopoverHolidaysServiceClient client =
				 new StopoverHolidaysServiceReference.StopoverHolidaysServiceClient())
			{
				//CHANGE THIS TO USE SERVICE.ISTRANSIT
				return client.IsTransit(routes);
			}
		}

		private Order createStubOrder(OrderRequests orderRequest)
		{
			var order = new Order
			{
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow,
				DeletedDate = null,
				CityId = 1,
				PartnerId = orderRequest.PartnerId.GetValueOrDefault(),
				Email = "",
				Referral = "",
				Passengers = orderRequest.NumberOfPassengers.GetValueOrDefault(),
				Language = "EN",
				MailSent = false,
				Comments = null
			};
			return order;
		}

		private OrderStopover createOrderStopover(OrderRequests orderRequest, Order order)
		{
			var transferForPassengerAmount = _context.Transfer.First(t => t.CityId == orderRequest.CityId &&
																			t.PassengerAmount == orderRequest.NumberOfPassengers);

			var orderStopover = new OrderStopover
			{
				CreatedDate = DateTime.UtcNow,
				UpdatedDate = DateTime.UtcNow,
				DeletedDate = null,
				Order = order,
				HotelId = orderRequest.HotelId.GetValueOrDefault(),
				TransferId = transferForPassengerAmount.Id,
				FromAirportTransferUsed = orderRequest.FromAirportTransferUsed.GetValueOrDefault(),
				FromHotelTransferUsed = orderRequest.FromHotelTransferUsed.GetValueOrDefault(),
				IsPromo = true, //change this once you clarify with Janna
				CheckIn = orderRequest.CheckIn.GetValueOrDefault(),
				CheckOut = orderRequest.CheckOut.GetValueOrDefault(),
				DayUse = orderRequest.DayUse.GetValueOrDefault(),
				Nights = orderRequest.Nights.GetValueOrDefault(),
				Price = 0,
				Comments = null
			};

			return orderStopover;
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

		private void createBookingReference(int requestId, Flight flight, OrderStopover orderStopover)
		{
			var bookingRefsToAdd = _context.OrderStopoverData.Select(b => new
			{
				b.OrderId,
				b.BookingReference,
			}).Where(b => b.OrderId == requestId).Distinct();

			foreach (var bookingRefToAdd in bookingRefsToAdd)
			{
				var bookingReference = new BookingReference
				{
					CreatedDate = DateTime.UtcNow,
					UpdatedDate = DateTime.UtcNow,
					DeletedDate = null,
					Flight = flight,
					PNR = bookingRefToAdd.BookingReference,
					Comments = null
				};

				_context.BookingReference.Add(bookingReference);
				createStopoverPassenger(requestId, bookingReference, orderStopover);
			}
		}

		private void createStopoverPassenger(int requestId, BookingReference bookingReference, OrderStopover orderStopover)
		{
			var passengersInRoomsToAdd = _context.OrderStopoverData.Select(p => new
			{
				p.OrderId,
				p.BookingReference,
				p.FirstName,
				p.LastName,
				p.IsChild,
				p.RoomNum,
				p.TicketNumber,
				p.RoomTypeId
			}).Where(p => p.OrderId == requestId).Where(p => p.BookingReference == bookingReference.PNR);

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

				var room = _context.Room.First(r => r.HotelId == orderStopover.HotelId && r.RoomTypeId == passenger.RoomTypeId);

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
					PromoUsed = true // AS TOLD BY JANNA THIS SHOULD BE TRUE REGARDLESS
				};

				_context.StopoverPassenger.Add(stopoverPassenger);
			}
		}

		private int calculateStopoverPrice(int requestId, OrderStopover os)
		{
		    int TotalPrice = 0;

            List<OrderStopoverData> spList = new List<OrderStopoverData>();

		    var stopoverData = _context.OrderStopoverData.Where(o => o.OrderId == requestId);

		    foreach (var sp in stopoverData)
		    {
		        if (!sp.IsChild.GetValueOrDefault())
		        {
		            if (promoUsedTicketNumber(sp.TicketNumber))
		            {
                        spList.Add(sp);
		            }
		            else
		            {
		                TotalPrice += sp.Price.Value;
                    }
		            
		        }

		    }

		    foreach (var sp in spList)
		    {
		        var paxInRoomWithPromoUsedPax = stopoverData.Where(sd => sd.RoomTypeId == sp.RoomTypeId &&
		                                                                 sd.RoomNum == sp.RoomNum &&
		                                                                 sd.FirstName != sp.FirstName &&
		                                                                 sd.LastName != sp.LastName).Distinct();
		        var room = _context.Room.First(r => r.HotelId == os.HotelId &&
		                                            r.RoomTypeId == sp.RoomTypeId);
                foreach (var spData in paxInRoomWithPromoUsedPax)
		        {
		            TotalPrice -= spData.Price.Value;
		        }


		        using (ServiceCurConverterClient client =
		            new ServiceCurConverterClient())
		        {
		            TotalPrice += (int)client.ConvertCurrency(room.FirstNightPrice, "KZT", "USD");
		            if (os.Nights > 1)
		            {
		                TotalPrice += (int)client.ConvertCurrency(room.SecondNightPrice, "KZT", "USD");
		            }
                }

		        
		            
		    }

			var transfer = _context.Transfer.FirstOrDefault(t => t.Id == os.TransferId);
		    using (ServiceCurConverterClient client = new ServiceCurConverterClient())
		    {
		        if (os.FromAirportTransferUsed && os.FromHotelTransferUsed)
		        {
		            TotalPrice += 2 * (int) client.ConvertCurrency(transfer.Price, "KZT", "USD");
		        }

		        if (os.FromAirportTransferUsed && !os.FromHotelTransferUsed)
		        {
		            TotalPrice += (int)client.ConvertCurrency(transfer.Price, "KZT", "USD"); 
		        }

		        if (!os.FromAirportTransferUsed && os.FromHotelTransferUsed)
		        {
		            TotalPrice += (int)client.ConvertCurrency(transfer.Price, "KZT", "USD");
                }
            }
			return TotalPrice;
		}

	    private bool promoUsedTicketNumber(string ticketNum)
	    {
	        var promoUsedPassenger = _context.StopoverPassenger.FirstOrDefault(sp => sp.TicketNumber == ticketNum &&
	                                                                                 sp.PromoUsed.HasValue);
	        if (promoUsedPassenger == null)
	        {
	            return false;
	        }

	        return promoUsedPassenger.PromoUsed.Value;
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