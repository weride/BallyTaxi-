using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Quickode.BallyTaxi.Core;
using System.Text;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Domain.Services;

namespace Quickode.BallyTaxi.API.Controllers
{
 
    public class BaseController : ApiController
    {
        public readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public BaseController()
        {
            
        }

        //public override Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        //{
           
        //    return base.ExecuteAsync(controllerContext, cancellationToken);
        //}

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            if (controllerContext.Request.RequestUri.PathAndQuery.Contains("/en/"))
                CultureHelper.SetCurrentCulture(System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            else
                CultureHelper.SetCurrentCulture(System.Globalization.CultureInfo.GetCultureInfo("he-IL"));


            var logEntry = new StringBuilder();

            if (controllerContext.Request != null)
            {
                logEntry.Append(controllerContext.Request.Method);
                logEntry.Append(":");
                logEntry.Append(controllerContext.Request.RequestUri);
                
                if (controllerContext.Request.Headers != null && controllerContext.Request.Headers.Authorization != null)
                {
                    logEntry.Append(",Auth[Schema:");
                    logEntry.Append(controllerContext.Request.Headers.Authorization.Scheme);
                    logEntry.Append(",Token:");
                    logEntry.Append(controllerContext.Request.Headers.Authorization.Parameter);
                    logEntry.Append("]");
                }

                Logger.Info(logEntry.ToString());
            }

            base.Initialize(controllerContext);

        }

        public static OrderDetailsModel FetchFullOrderDetails(Order order)
        {
            if (order == null)
                return null;

            var driver = order.DriverId.HasValue ? DriverService.FetchDriver(order.DriverId.Value) : (Driver)null;
            var station = driver != null && driver.TaxiStationId.HasValue ? DriverService.GetTaxiStationById(driver.TaxiStationId.Value) : (TaxiStation) null;
            var car = driver != null && driver.CarType.HasValue ? DriverService.getCarTypeById(driver.CarType.Value) : null;
            var driverUser = order.DriverId.HasValue ? UserService.FetchUser(order.DriverId.Value) : null;
            var passenger = PassengerService.FetchPassenger(order.PassengerId);
            var passengerUser = passenger != null ? UserService.FetchUser(passenger.UserId) : null;
            double InterCityPrice = 0;
            if(order.isWithDiscount==true )
            //if (order.destinationCityName != order.pickUpCityName && order.Driver!=null && order.StatusId==(int)DriverStatus.OnTheWayToDestination && order.Driver.TaxiStationId == 338 /*מוניות הדקה ה99*/)
            {
                //var model = new Models.Models.LocationForIntercityTravel();

                //model.destinationCityName = order.destinationCityName;
                //model.destinationLatitude = order.DestinationLocation.Latitude.Value;
                //model.destinationLongitude = order.DestinationLocation.Longitude.Value;
                //model.lat = order.PickUpLocation.Latitude.Value;
                //model.lon = order.PickUpLocation.Longitude.Value;
                //model.pickUpCityName = order.pickUpCityName;
                //var result = OrderService.CalcPriceIntercityTravel(model);
                //if(result!=null && result.Count>0)
                //InterCityPrice = result["priceAfterDiscount"];
                InterCityPrice = order.priceForInterCity.HasValue ? order.priceForInterCity.Value : 0;
            }

            OrderDetailsModel order_result = new OrderDetailsModel(order, driver, driverUser, passengerUser, station,InterCityPrice, car);
            return order_result;
        }

    }
}
