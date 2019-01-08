﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class StopoverDbContext : DbContext
    {
        public StopoverDbContext()
            : base("name=StopoverDbContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ActivityPassenger> ActivityPassenger { get; set; }
        public virtual DbSet<ActivityPriceType> ActivityPriceType { get; set; }
        public virtual DbSet<ActivityTime> ActivityTime { get; set; }
        public virtual DbSet<BookingReference> BookingReference { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<ExtraRoomPrice> ExtraRoomPrice { get; set; }
        public virtual DbSet<Flight> Flight { get; set; }
        public virtual DbSet<Hotel> Hotel { get; set; }
        public virtual DbSet<HotelBlockedDay> HotelBlockedDay { get; set; }
        public virtual DbSet<HotelPromoDisable> HotelPromoDisable { get; set; }
        public virtual DbSet<Passenger> Passenger { get; set; }
        public virtual DbSet<PromoUsedBookingReference> PromoUsedBookingReference { get; set; }
        public virtual DbSet<RoomType> RoomType { get; set; }
        public virtual DbSet<Session> Session { get; set; }
        public virtual DbSet<Translation> Translation { get; set; }
        public virtual DbSet<ActivityBlockedDay> ActivityBlockedDay { get; set; }
        public virtual DbSet<Activity> Activity { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderActivity> OrderActivity { get; set; }
        public virtual DbSet<Partner> Partner { get; set; }
        public virtual DbSet<FlightPassenger> FlightPassenger { get; set; }
        public virtual DbSet<StopoverPassenger> StopoverPassenger { get; set; }
        public virtual DbSet<Transfer> Transfer { get; set; }
        public virtual DbSet<Room> Room { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<OrderStopover> OrderStopover { get; set; }
        public virtual DbSet<ActivityPrice> ActivityPrice { get; set; }
        public virtual DbSet<OrderActivityData> OrderActivityData { get; set; }
        public virtual DbSet<OrderCost> OrderCost { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<OrderRequests> OrderRequests { get; set; }
        public virtual DbSet<OrderStopoverData> OrderStopoverData { get; set; }
        public virtual DbSet<StopoverSearch_v> StopoverSearch_v { get; set; }
        public virtual DbSet<database_firewall_rules> database_firewall_rules { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AuditEntries> AuditEntries { get; set; }
        public virtual DbSet<AuditEntryProperties> AuditEntryProperties { get; set; }
    }
}
