using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Domain.Services;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Models;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("business")]
    public class BusinessController : BaseController
    {
        [Route("l2")]
        [HttpGet]
        public HttpResponseMessage List2()
        {

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [Route("list")]
        [HttpGet]
        public HttpResponseMessage ListOfBusinesses(string isoCountry = null, double lat = 0, double lon = 0)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                if (isoCountry == null || isoCountry == "null")
                    isoCountry = BusinessServices.getCityfromLocation(lat, lon);
                if (isoCountry == ""|| isoCountry == null)
                    isoCountry = "IL";//default is Israel
                var businesses = BusinessServices.GetListOfBusiness(isoCountry);

                var result = new List<BusinessModel>();
                foreach (var b in businesses)
                    result.Add(new BusinessModel()
                    {
                        BusinessId = b.BusinessId,
                        BusinessName = b.BusinessName,
                        Phone = b.Phone,
                        isNeedFile=b.isNeedFile==true?true:false
                    });

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }

        [Route("add")]
        [HttpPut]
        public HttpResponseMessage AddBusiness([FromBody] BusinessModel model, string isoCountry)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var b = BusinessServices.AddBusiness(isoCountry, model.BusinessName, model.PayPalAccount, model.Phone, model.isNeedFile);

                if (b != null)
                {
                    var result = new BusinessModel()
                    {
                        BusinessId = b.BusinessId,
                        BusinessName = b.BusinessName,
                        PayPalAccount = b.PayPalAccountId,
                        Phone = b.Phone,
                        isNeedFile=b.isNeedFile==true?true:false
                    };
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Error creating business"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }

        [Route("del")]
        [HttpDelete]
        public HttpResponseMessage DeleteBusiness(string isoCountry, string businessName)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var b = BusinessServices.DeleteBusiness(isoCountry, businessName);

                if (b)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Error delete business"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }

        [Route("upd")]
        [HttpPatch]
        public HttpResponseMessage UpdateBusiness([FromBody] BusinessModel model, string isoCountry)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var b = BusinessServices.UpdateBusiness(isoCountry, model.BusinessName, model.PayPalAccount, model.Phone);

                if (b)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Error update business"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }

        [Route("person/add")]
        [HttpPut]
        public HttpResponseMessage AddPerson([FromBody]BusinessPersonModel model )
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var b = BusinessServices.AddPerson(model.IsoCountry, model.BusinessName, model.Phone);

                if (b)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Error update business"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }

        [Route("person/del")]
        [HttpDelete]
        public HttpResponseMessage DeletePerson([FromBody]BusinessPersonModel model)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var b = BusinessServices.DeletePerson(model.IsoCountry, model.BusinessName, model.Phone);

                if (b)
                    return Request.CreateResponse(HttpStatusCode.OK);

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Error update business"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }

        [Route("person/check")]
        [HttpPost]
        public HttpResponseMessage CheckPerson([FromBody]BusinessPersonModel model)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var b = BusinessServices.CheckPerson(model.IsoCountry, model.BusinessName, model.Phone, user.UserId);

                
                return Request.CreateResponse(HttpStatusCode.OK, b);

                //return Request.CreateErrorResponse(HttpStatusCode.BadRequest, new HttpError("Error update business"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }
    }




    
}
