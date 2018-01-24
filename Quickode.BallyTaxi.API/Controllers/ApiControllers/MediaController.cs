using Quickode.BallyTaxi.Domain.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Models;
using System.IO;
using System.Net.Http.Headers;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("media")]
    public class MediaController : ApiController
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //[Route("image", Order = 1)]
        //[HttpGet]
        //public HttpResponseMessage GetImage(Guid imageID)
        //{
        //    try
        //    {
        //        var baseCon = new BaseController();
        //        baseCon.Logger.DebugFormat("media - imageId {0}", imageID.ToString());
        //        var image = MediaService.GetImage(imageID);
        //        byte[] content = MediaService.GetImageContent(image);
        //        string imgUrl = MediaService.GetImageUrl(image);
        //        baseCon.Logger.DebugFormat("media - content {0}", content.ToString());
        //        //if (content != null)
        //        //{

        //        baseCon.Logger.DebugFormat("media - content is not null {0}", content.ToString());
        //        HttpResponseMessage response = new HttpResponseMessage();
        //        response.Content = new ByteArrayContent(content);
        //        response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + image.Extension);
        //        response.StatusCode = HttpStatusCode.OK;
        //        return response;
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("GetImage", ex);
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError(ex.Message));
        //    }
        //    // throw new HttpResponseException(HttpStatusCode.NotFound);
        //}

        [Route("image", Order = 1)]
        [HttpGet]

        public HttpResponseMessage GetImage(Guid imageID)
        {
            var baseCon = new BaseController();
            baseCon.Logger.DebugFormat("media - imageId {0}", imageID.ToString());
            var image = MediaService.GetImage(imageID);
            byte[] content = MediaService.GetImageContent(image);
            if (content != null && content.Length > 0)
            {
                baseCon.Logger.DebugFormat("media - content is not null {0}", content.ToString());
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + image.Extension);
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            else
            {
                baseCon.Logger.DebugFormat("media - content is null ");
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "media - content is not found");
            }
            // throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [Route("getImage", Order = 1)]
        [HttpGet]
        public HttpResponseMessage GetImageById(Guid imageID)
        {
            var baseCon = new BaseController();
            baseCon.Logger.DebugFormat("media - imageId {0}", imageID.ToString());
            var image = MediaService.GetImage(imageID);
            string imageUrl = MediaService.GetImageUrl(image);
            if (imageUrl != null)
            {
                //baseCon.Logger.DebugFormat("media - imageUrl {0}", imageUrl);
                //HttpResponseMessage response = new HttpResponseMessage();
                //response.Content = content;
                //response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + image.Extension);
                //response.StatusCode = HttpStatusCode.OK;
                //return response;
                return Request.CreateResponse(HttpStatusCode.OK, imageUrl);
            }
            else
            {
                baseCon.Logger.DebugFormat("media - imageUrl not found");
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "media - content is not found");
            }
            // throw new HttpResponseException(HttpStatusCode.NotFound);
        }


        [Route("flag")]
        [HttpGet]
        public HttpResponseMessage GetFlagImage(string iso)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "CountriesFlags" + "\\" + iso.Replace("\"", "").ToUpperInvariant() + ".png"; ;

            if (File.Exists(path))
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new ByteArrayContent(File.ReadAllBytes(path));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                response.StatusCode = HttpStatusCode.OK;
                return response;
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [Route("upload")]
        [HttpPost]
        public HttpResponseMessage UploadImage(UserImageModel image)
        {
            Logger.DebugFormat("UploadImage  start");
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                Guid imageId = user.ImageId!=null?(Guid)user.ImageId : Guid.Empty; ;
                var imageID = MediaService.SaveImage(image.Base64, image.Extension, imageId, true);
                if (user.ImageId == null)
                   UserService.UpdateUserImageId(user.UserId, imageID);
                return Request.CreateResponse(HttpStatusCode.OK, imageID);
            }
            catch (UserBlockedException ex)
            {
                Logger.Error("UploadImage", ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.Error("UploadImage", ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.Error("UploadImage", ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Error("UploadImage", ex);
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError(ex.Message));
            }
        }
    }
}
