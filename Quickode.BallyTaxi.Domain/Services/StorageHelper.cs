using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace Quickode.BallyTaxi.Domain.Services
{
    public class StorageHelper
    {
        public string Folder { get; private set; }
        public string folderToPath { get; private set; }
        public string folderUploadPath { get; private set; }
        private bool IsFS { get; set; }
        private CloudBlobClient blobStorage;
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StorageHelper(string folder, string folderToPath, string folderUploadPath)
        {
            this.Folder = folder;
            this.folderToPath = folderToPath;
            this.folderUploadPath = folderUploadPath;
            IsFS = true; //Path.IsPathRooted(folder);//******fix
            //if (IsFS && !Directory.Exists(folder))
            //    Directory.CreateDirectory(folder);

            if (!IsFS)
            {
                try
                {
                    StorageCredentials creds = new StorageCredentials(ConfigurationHelper.StorageAccount, ConfigurationHelper.StorageKey);
                    blobStorage = new CloudBlobClient(new Uri(ConfigurationHelper.StorageUrl), creds);
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        public string GetPath(string filename)
        {
            if (!IsFS)
                throw new InvalidOperationException();
            return Path.Combine(Folder, filename);
        }

        private string GetToPath(string filename)
        {
            if (!IsFS)
                throw new InvalidOperationException();
            Logger.Debug("file path: "+ Path.Combine(folderToPath, filename));
            return Path.Combine(folderToPath, filename);
        }

        public byte[] GetFile(string filename)
        {
            if (IsFS)
            {
                try
                {
                    string path = GetPath(filename);
                    var webClient = new WebClient();
                    byte[] imageBytes = webClient.DownloadData(path);
                    return imageBytes;

                    //if (File.Exists(GetPath(filename))) ///********fix?
                    //    return File.ReadAllBytes(GetPath(filename));
                }
                catch (Exception e)
                {
                    Logger.Error("GetFile", e);
                    return null;
                }
            }
            else
            {
                CloudBlobContainer container = blobStorage.GetContainerReference(Folder);
                CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                if (blob == null)
                    return null;
                MemoryStream result = new MemoryStream();
                blob.DownloadToStream(result);
                result.Position = 0;
                return result.ToArray();

            }
        }

        public void SaveFile(string filename, string Extension, byte[] content)
        {
            if (IsFS)
            {
                try
                {
                    string filePath = GetToPath(filename);

                    File.WriteAllBytes(filePath, content); //*********FIX
                    Logger.Debug("SaveFile " + filePath);

                    //save image in Amazon
                    WritingAnObject(filename, Extension);


                    //  Console.WriteLine("Press any key to continue...");
                    //   Console.ReadKey();



                    // System.Diagnostics.Process process = new System.Diagnostics.Process();
                    // System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                    // startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    // startInfo.RedirectStandardInput = true;
                    // startInfo.RedirectStandardOutput = true;    
                    // startInfo.UseShellExecute = false;
                    // //startInfo.CreateNoWindow = false;
                    // startInfo.FileName = "cmd.exe";
                    // startInfo.Arguments = string.Format("/C aws s3 cp {0}\\{1} {2}{3}", folderToPath, filename, folderUploadPath, filename);
                    ////startInfo.Arguments = "/C dir";
                    // process.StartInfo = startInfo;
                    // process.Start();
                    // process.WaitForExit();
                    // string output = process.StandardOutput.ReadToEnd();

                    //"aws s3 cp C:\Quickode\BallyTaxi\server\Quickode.BallyTaxi.API\ProfileImages\00a8eeeb-8347-412bbde5-fa119737c83f.png s3://riderimages/00a8eeeb-8347-412b-bde5-fa119737c83f.png"


                    //var webClient = new WebClient();
                    // byte[] imageBytes = webClient.uploadf(GetPath(filename), content);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("SaveFile", e.Message);
                }
            }
            else
            {
                CloudBlobContainer container = blobStorage.GetContainerReference(Folder);
                CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                blob.UploadFromByteArray(content, 0, content.Length);
                Logger.ErrorFormat("SaveFile UploadFromByteArray");


            }
        }
        public void DeleteFile(string filename)
        {
            if (IsFS && File.Exists(GetPath(filename)))
            {
                File.Delete(GetPath(filename));
            }
            else
            {
                CloudBlobContainer container = blobStorage.GetContainerReference(Folder);
                CloudBlockBlob blob = container.GetBlockBlobReference(filename);
                blob.DeleteIfExists();
            }
        }

        static void WritingAnObject(string fileName, string Extension)
        {
            try
            {
                using (var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
                {
                    //PutObjectRequest putRequest1 = new PutObjectRequest
                    //{
                    //    BucketName = fileName,
                    //    Key = keyName,
                    //    ContentBody = "sample text"
                    //};

                    //PutObjectResponse response1 = client.PutObject(putRequest1);

                    // 2. Put object-set ContentType and add metadata.
                    PutObjectRequest putRequest2 = new PutObjectRequest
                    {
                        BucketName = ConfigurationHelper.BucketName,
                        Key = fileName,
                        FilePath = ConfigurationHelper.MediaFromFolderPath + "\\" + fileName,
                        ContentType = "image/" + Extension
                    };
                    //putRequest2.Metadata.Add("x-amz-meta-title", "someTitle");

                    PutObjectResponse response2 = client.PutObject(putRequest2);
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                Logger.Error("WritingAnObject", amazonS3Exception);
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    //Console.WriteLine("Check the provided AWS Credentials.");
                    //Console.WriteLine(
                    //   "For service sign up go to http://aws.amazon.com/s3");

                }
                else
                {
                    //Console.WriteLine(
                    //  "Error occurred. Message:'{0}' when writing an object"
                    //  , amazonS3Exception.Message);
                }
            }
        }

    }
}