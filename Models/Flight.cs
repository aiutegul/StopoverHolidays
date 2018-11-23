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
    
    public partial class Flight
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Flight()
        {
            this.BookingReference = new HashSet<BookingReference>();
        }
    
        public int Id { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public int OrderStopoverId { get; set; }
        public System.DateTime ArriveDate { get; set; }
        public System.DateTime DepartureDate { get; set; }
        public string ArriveFlight { get; set; }
        public string DepartureFlight { get; set; }
        public string Routes { get; set; }
        public bool IsTransit { get; set; }
        public bool IsPointToPoint { get; set; }
        public string Comments { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookingReference> BookingReference { get; set; }
        public virtual OrderStopover OrderStopover { get; set; }
    }
}