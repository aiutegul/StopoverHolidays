using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace StopoverAdminPanel.Models
{
    public class StopoverData
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isChild { get; set; }
        [Display(Name = "City")]
        public int CityId { get; set; }
        public string BookingReference { get; set; }
        public string TicketNumber { get; set; }
        [Display(Name = "Hotel")]
        public int HotelId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        [Display(Name = "Room Type")]
        public int RoomTypeId { get; set; }
        public int TransferId { get; set; }
        public bool FromAirportTransferUsed { get; set; }
        public bool FromHotelTransferUsed { get; set; }
        public DateTime ArriveDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string ArriveFlight { get; set; }
        public string DepartureFlight { get; set; }
        public string Routes { get; set; }
        public bool IsTransit { get; set; }
        public bool IsPointToPoint { get; set; }
        public bool DayUse { get; set; }
        public bool IsPromo { get; set; }
        public int Nights { get; set; }
        public int OrderStopoverPrice { get; set; }
        public bool PromoUsed { get; set; }
        public int RoomNum { get; set; }
    }
}