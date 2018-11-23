//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StopoverAdminPanel.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class StopoverPassenger
    {
        public int Id { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public int RoomId { get; set; }
        public int RoomNum { get; set; }
        public int OrderStopoverId { get; set; }
        public int PassengerId { get; set; }
        public int BookingReferenceId { get; set; }
        public string TicketNumber { get; set; }
        public Nullable<bool> PromoUsed { get; set; }
        public string Comments { get; set; }
    
        public virtual BookingReference BookingReference { get; set; }
        public virtual Passenger Passenger { get; set; }
        public virtual Room Room { get; set; }
        public virtual OrderStopover OrderStopover { get; set; }
    }
}