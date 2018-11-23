using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StopoverAdminPanel.Models
{
    public class ActivityData
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isChild { get; set; }
        public int ActivityId { get; set; }
        public int ActivityTimeId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string TransferLocation { get; set; }
        public int TotalPrice { get; set; }
    }
}