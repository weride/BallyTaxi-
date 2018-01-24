using System;
using Quickode.BallyTaxi.Core;

namespace Quickode.BallyTaxi.Models
{
    public class LocationObject
    {
        public double? lat { get; set; }
        public double? lon { get; set; }
        public string address { get; set; }
        public string CityName { get; set; }

        public LocationObject()
        {

        }

    }

    public class IVROrderModel
    {
        public string phone { get; set; }
        public string address { get; set; }
        public double orderTime { get; set; }
        public bool isAirport { get; set; }
    }

    public class OrderTimeModel
    {
        public double lat { get; set; }
        public double lon { get; set; }
        public string estimateTime { get; set; }

    }

    public class EstimationTimeModel
    {
        public double pickupLatitude { get; set; }
        public double pickupLongitude { get; set; }
        public double destinationLatitude { get; set; }
        public double destinationLongitude { get; set; }
        public double time { get; set; }

    }



    public class DestinationLocationModel
    {
        public long rideID;
        public double destinationLatitude;
        public double destinationLongitude;
        public string destinationAddress;
        public string destinationCity;
    }

    public class OrderModel
    {
        public double pickupLatitude;
        public double pickupLongitude;
        public string pickupAddress;
        public string pickUpCityName { get; set; }

        public double destinationLatitude;
        public double destinationLongitude;
        public string destinationAddress;
        public string destinationCityName { get; set; }
        public string notes;
        public double time;
        public string fileNumber;

        public int paymentMethod; //PaymentMethod 
        public bool? isInterCity;
        public bool isFromWeb;
        public bool isFromStations;
        public int businessId { get; set; }
        public bool isWithDiscount { get; set; }
        public int seatsNumber { get; set; }
        public int? courier { get; set; }
        public bool? isHandicapped { get; set; }
        public long accountId { get; set; }
        public int roleId { get; set; }
        public OrderModel()
        {

        }
    }

    public class VirtualOrderModel
    {
        public double pickupLatitude;
        public double pickupLongitude;

        public double time;
        public int paymentMethod = (int)CustomerPaymentMethod.Cash; //PaymentMethod 


        public VirtualOrderModel()
        {

        }
    }

    public class OrderDetailsModel
    {

        public long orderID { get; set; }
        public double? pickupLatitude { get; set; }
        public double? pickupLongitude { get; set; }
        public string pickupAddress { get; set; }
        public double? destinationLatitude { get; set; }
        public double? destinationLongitude { get; set; }
        public string destinationAddress { get; set; }
        public string notes { get; set; }
        public double? time { get; set; }
        public bool isFutureRide { get; set; }
        public int type { get; set; }
        public double InterCityPrice { get; set; }
        public bool isWithDiscount { get; set; }
        public int orderStatus { get; set; }
        public int seatsNumber { get; set; }
        public int courier { get; set; }
        public bool isHandicapped { get; set; }
        public bool isVirtual { get; set; }
        public double? amount { get; set; }

        public DriverProfileToDisplay driverObject { get; set; }
        public PassengerProfileToDisplay passengerObject { get; set; }

        public OrderDetailsModel(Order order, Driver driver, User driverUser, User passengerUser, TaxiStation station)
        {
            orderStatus = order.StatusId;
            orderID = order.OrderId;
            destinationAddress = order.DestinationAddress;
            destinationLatitude = order.DestinationLocation.Latitude;
            destinationLongitude = order.DestinationLocation.Longitude;
            pickupAddress = order.PickUpAddress;
            pickupLatitude = order.PickUpLocation.Latitude;
            pickupLongitude = order.PickUpLocation.Longitude;
            notes = order.Notes;
            isWithDiscount = order.isWithDiscount.HasValue ? order.isWithDiscount.Value : false;
            seatsNumber = order.seats.HasValue ? order.seats.Value : 4;
            courier = order.courier.HasValue ? order.courier.Value : 0;
            isHandicapped = order.isHandicapped.HasValue ? order.isHandicapped.Value : false;
            isVirtual = order.isVirtual == true ? true : false;
            this.amount = order.Amount;

            DateTime.Now.ConvertToUnixTimestamp();
            if (order.OrderTime.HasValue)
            {
                time = order.OrderTime.Value.ConvertToUnixTimestamp();
                isFutureRide = (order.OrderTime.Value - DateTime.UtcNow).TotalMinutes <= 5 ? false : true;
                type = order.StatusId == (int)OrderStatus.Pending ? (int)OrderType.Search : ((order.OrderTime.Value - DateTime.UtcNow).TotalHours < 0 ? (int)OrderType.Past : (order.OrderTime.Value - DateTime.UtcNow).TotalHours < 1 ? (int)OrderType.Current : (int)OrderType.Future);
            }
            else
            {
                time = null;
                if (order.StatusId == (int)OrderStatus.Pending)
                    type = (int)OrderType.Search;

            }

            if (driver != null)
            {
                driverObject = new DriverProfileToDisplay(driver, driverUser, station, null);
            }
            else
                driverObject = null;

            if (passengerUser != null)
            {
                passengerUser.PreferredPaymentMethod = order.PaymentMethod;
                passengerObject = new PassengerProfileToDisplay(passengerUser);
            }
            else
                passengerObject = null;
        }
        public OrderDetailsModel(Order order, Driver driver, User driverUser, User passengerUser, TaxiStation station, double InterCityPrice, CarType car = null)
        {
            orderStatus = order.StatusId;
            orderID = order.OrderId;
            destinationAddress = order.DestinationAddress;
            destinationLatitude = order.DestinationLocation.Latitude;
            destinationLongitude = order.DestinationLocation.Longitude;
            pickupAddress = order.PickUpAddress;
            pickupLatitude = order.PickUpLocation.Latitude;
            pickupLongitude = order.PickUpLocation.Longitude;
            notes = order.Notes;
            this.isWithDiscount = order.isWithDiscount.HasValue ? order.isWithDiscount.Value : false;
            this.InterCityPrice = InterCityPrice;
            seatsNumber = (order.seats.HasValue && order.seats.Value > 0) ? order.seats.Value : 4;
            courier = order.courier.HasValue ? order.courier.Value : 0;
            isHandicapped = order.isHandicapped.HasValue ? order.isHandicapped.Value : false;
            isVirtual = order.isVirtual == true ? true : false;
            this.amount = order.Amount;

            if (order.OrderTime.HasValue)
            {
                time = order.OrderTime.Value.ConvertToUnixTimestamp();
                isFutureRide = (order.OrderTime.Value - DateTime.UtcNow).TotalMinutes <= 5 ? false : true;
                type = order.StatusId == (int)OrderStatus.Pending ? (int)OrderType.Search : ((order.OrderTime.Value - DateTime.UtcNow).TotalHours < 0 ? (int)OrderType.Past : (order.OrderTime.Value - DateTime.UtcNow).TotalHours < 1 ? (int)OrderType.Current : (int)OrderType.Future);
            }
            else
            {
                time = null;
                if (order.StatusId == (int)OrderStatus.Pending)
                    type = (int)OrderType.Search;
            }

            if (driver != null)
            {
                driverObject = new DriverProfileToDisplay(driver, driverUser, station, car);
            }
            else
                driverObject = null;

            if (passengerUser != null)
            {
                passengerUser.PreferredPaymentMethod = order.PaymentMethod;
                passengerObject = new PassengerProfileToDisplay(passengerUser, order: order);
            }
            else
                passengerObject = null;
        }

        public OrderDetailsModel()
        {

        }
    }

    public class SummaryRideObject
    {
        public string pickupAddress { get; set; }
        public string destinationAddress { get; set; }
        public string pickupCity { get; set; }
        public string destinationCity { get; set; }
        public double? endTime { get; set; }
        public int rating { get; set; }
        public long orderID { get; set; }
        public double? time { get; set; }
        public int type { get; set; }
        public int paymentMethod { get; set; }
        public string passengerName { get; set; }
        public string PassengerPhone { get; set; }
        public string notes { get; set; }
        public double amount { get; set; }
        public double distance { get; set; }
        public string driverName { get; set; }
        public string driverPhone { get; set; }

        public SummaryRideObject(Order order)
        {
            orderID = order.OrderId;
            destinationAddress = order.DestinationAddress;
            pickupAddress = order.PickUpAddress;
            pickupCity = order.pickUpCityName;
            destinationCity = order.destinationCityName;
            rating = order.Rating.HasValue ? order.Rating.Value : 0;
            paymentMethod = order.PaymentMethod != null ? order.PaymentMethod.Value : 0;
            notes = order.Notes;
            amount = order.Amount.HasValue ? order.Amount.Value : 0;

            var locA = new System.Device.Location.GeoCoordinate((double)order.PickUpLocation.Latitude, (double)order.PickUpLocation.Longitude);
            var locB = new System.Device.Location.GeoCoordinate((double)order.DestinationLocation.Latitude, (double)order.DestinationLocation.Longitude);
            double distance1 = locA.GetDistanceTo(locB); // metres

            distance = distance1 / 1000;// km
            var a = Math.Round((Convert.ToDecimal(distance)), 2);
            distance = Convert.ToDouble(a);

            if (order.OrderTime.HasValue)
            {
                time = order.OrderTime.Value.ConvertToUnixTimestamp();
                type = order.StatusId == (int)OrderStatus.Pending ? (int)OrderType.Search : ((order.OrderTime.Value - DateTime.UtcNow).TotalHours < 0 ? (int)OrderType.Past : (order.OrderTime.Value - DateTime.UtcNow).TotalHours < 1 ? (int)OrderType.Current : (int)OrderType.Future);
            }
            else
            {
                time = null;
                if (order.StatusId == (int)OrderStatus.Pending) type = (int)OrderType.Search;
            }
            if (order.EndTime.HasValue)
                endTime = order.EndTime.Value.ConvertToUnixTimestamp();
            else endTime = null;
            if (order.Passenger != null)
            {
                passengerName = order.Passenger.Name;
                PassengerPhone = order.Passenger.Phone;
            }
            if (order.Driver != null)
            {
                if (order.Driver.User != null)
                {
                    driverName = order.Driver.User.Name;
                    driverPhone = order.Driver.User.Phone;
                }
            }
        }
    }

    public class EndRideModel
    {
        public long orderId { get; set; }
        //public Nullable<int> PaymentMethod { get; set; }
        public int paymentMethod { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public long cardId { get; set; }
        public long userId { get; set; }
        public bool setDefaultCard { get; set; }
        public bool alwaysApprovePayment { get; set; }
        public string fileNumber { get; set; }

    }

    public class EndsRideModel {
        public long orderId { get; set; }
        public int paymentMethod { get; set; }
        public int rating { get; set; }
        public int tip { get; set; }
        public double? amount { get; set; }
        public string currency { get; set; }
        public string FileNumber { get; set; }

    }

    public class EndRidePassengerModel
    {
        public long orderId { get; set; }
        public int rating { get; set; }
        public int tip { get; set; }
        public int paymentMethod { get; set; }
    }
}
