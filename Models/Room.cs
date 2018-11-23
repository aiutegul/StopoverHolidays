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
    
    public partial class Room
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Room()
        {
            this.ExtraRoomPrice = new HashSet<ExtraRoomPrice>();
            this.StopoverPassenger = new HashSet<StopoverPassenger>();
        }
    
        public int Id { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public Nullable<System.DateTime> DeletedDate { get; set; }
        public int HotelId { get; set; }
        public int RoomTypeId { get; set; }
        public int FirstNightPrice { get; set; }
        public int SecondNightPrice { get; set; }
        public int FirstNightPriceNet { get; set; }
        public int SecondNightPriceNet { get; set; }
        public int PromoNetPrice { get; set; }
        public string Comments { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExtraRoomPrice> ExtraRoomPrice { get; set; }
        public virtual Hotel Hotel { get; set; }
        public virtual RoomType RoomType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StopoverPassenger> StopoverPassenger { get; set; }
    }
}
