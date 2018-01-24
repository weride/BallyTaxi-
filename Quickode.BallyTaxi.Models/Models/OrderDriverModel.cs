using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Models
{
    public class OrderDriverModel
    {
        public long OrderId { get; set; }
        public long DriverId { get; set; }
        public int StatusId { get; set; }
        public int Priority { get; set; }

        public OrderDriverModel() { }
        public OrderDriverModel(Orders_Drivers od)
        {
            OrderId = od.OrderId;
            DriverId = od.DriverId;
            StatusId = od.StatusId;
            Priority = od.Priority;
        }
    }
}