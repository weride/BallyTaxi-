using Quickode.BallyTaxi.Domain.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Models;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("contents")]
    public class ContentController : BaseController
    {

        [Route("getCountryCodes")]
        [HttpGet]
        public HttpResponseMessage GetCountryCodes(int userType, int languageId)
        {
            try
            {
                Logger.Debug("GetCountryCodes start");
                List<object> supportedCountry_list = new List<object>();
                List<object> unSupportedCountry_list = new List<object>();
                var result_db = ContentService.GetCountryData();
               
                foreach (var item in result_db)
                {
                    var obj = new { flagURL = this.Url.Link("Default", new { Controller = "Media", Action = "flag", iso = item.ISO31661Alpha2 }), name = languageId==(int)UserLanguages.he?item.hebrewName: item.name, ISOCode = item.ISO31661Alpha2, phonePrefix = item.Dial };
                    if(ContentService.IsSupportedCountry(item.ISO31661Alpha2))
                    supportedCountry_list.Add(obj);
                    else unSupportedCountry_list.Add(obj);
                }
                if (userType == (int)UserType.Driver)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { arrSupportedCountries = supportedCountry_list  }); 

                }
                else if (userType == (int)UserType.Passenger) {
                    return Request.CreateResponse(HttpStatusCode.OK, new { arrSupportedCountries = supportedCountry_list, arrOtherCountries = unSupportedCountry_list }); 
                }
                else {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Bad Request - error in request params"));
                }
            }
            
            catch (Exception ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)500, new HttpError(ex.InnerException.Message));
            } 
        } 
         
    }
}
