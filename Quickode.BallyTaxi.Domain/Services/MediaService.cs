using Quickode.BallyTaxi.Models;
using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Text;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class MediaService
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static StorageHelper storageHelper = new StorageHelper(ConfigurationHelper.MediaFolderPath, ConfigurationHelper.MediaFromFolderPath, ConfigurationHelper.MediaToFolderPath);

        public static byte[] GetImageContent(Image image)
        {
            if (image == null)
                return null;
            return storageHelper.GetFile(image.Filename);
        }
        public static string GetImageUrl(Image image)
        {
            if (image == null)
                return null;
            return storageHelper.GetPath(image.Filename);
        }
        public static Image GetImage(Guid ImageId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.Images.Where(x => x.ImageId.Equals(ImageId)).FirstOrDefault();
            }
        }

        public static List<Image> getAllImagesFromServer()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var imagesForUsers = db.Users.Select(u => u.ImageId).ToList();
                var images = db.Images.Where(i => imagesForUsers.Contains(i.ImageId)).ToList();
                return images;
            }
        }

        public static Guid SaveImage(string imageBase64, string extension, Guid ImageId, bool ToFS)
        {
            Logger.DebugFormat("extension: {0}", extension);
            Image image = null;
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                image = db.Images.Where(x => x.ImageId.Equals(ImageId)).FirstOrDefault();
                Logger.DebugFormat("SaveImage ImageId: {0}", ImageId);
                if (ImageId == Guid.Empty|| image==null)
                    {
                        image = db.Images.Create();
                        image.ImageId = Guid.NewGuid();
                        image.Extension = extension;
                        db.Images.Add(image);
                    }
                else 
                    image.Extension = extension;
                db.SaveChanges();
                }            
          
            //save image
            if (image != null && ToFS)
            {
                // imageBase64 = imageBase64.Replace(" ", "");
                // byte[] byt = System.Text.Encoding.UTF8.GetBytes(imageBase64);
                //  var imageBase64A = imageBase64.Substring(0, imageBase64.Length / 200);
                //   var imageBase64B = imageBase64.Substring(imageBase64.Length / 2000);
                var byte1 = Convert.FromBase64String(imageBase64);
                //   var byte2= Convert.FromBase64String(imageBase64B);
                // byte[] rv = new byte[byte1.Length + byte2.Length ];
                // System.Buffer.BlockCopy(byte1, 0, rv, 0, byte1.Length);
                // System.Buffer.BlockCopy(byte2, 0, rv, byte1.Length, byte2.Length);
                storageHelper.SaveFile(image.Filename, image.Extension, byte1);//Convert.FromBase64String(imageBase64));
                return image.ImageId;
            }
            return image != null ? image.ImageId : Guid.Empty;
        }
    }

}