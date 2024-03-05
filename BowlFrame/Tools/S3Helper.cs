using Amazon.S3.Model;
using Amazon.S3;
using static BowlFrame.Tools.Tools;
using static BowlFrame.Tools.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BowlFrame.Config;
using Newtonsoft.Json.Linq;
using System.Net.Sockets;
using Amazon.Runtime.Internal;

namespace BowlFrame.Tools
{
    internal class S3Helper
    {
        private readonly string Accesskey;
        private readonly string Secretkey;
        private readonly string Endpoint;
        private readonly string Bucket;

        private readonly AmazonS3Client _client;

        public S3Helper()
        {
            JObject config = JObject.Parse(ConfigLoad.GetFileString("S3.json") ?? throw new NullReferenceException());

            Accesskey = (string?)config["Accesskey"] ?? throw new NullReferenceException();
            Secretkey = (string?)config["Secretkey"] ?? throw new NullReferenceException();
            Endpoint = (string?)config["Endpoint"] ?? throw new NullReferenceException();
            Bucket = (string?)config["Bucket"] ?? throw new NullReferenceException();

            AmazonS3Config s3Config = new()
            {
                ServiceURL = Endpoint,
            };

            _client = new(Accesskey, Secretkey, s3Config);
        }

        public S3Helper(string bucket) : this()
        {
            Bucket = bucket;
        }

        public async Task<(bool, string?)?> UploadFile(string key, string filePath)
        {
            if (await IsHasFile(key) == true) return (true, key);

            try
            {
                PutObjectRequest request = new()
                {
                    BucketName = Bucket,
                    Key = key,
                    FilePath = filePath
                };

                PutObjectResponse response = await _client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return (true, request.Key);

                return (false, null);
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return null;
            }
        }

        public async Task<(bool, string?)?> UploadFile(string key, Stream fileStream, string? ContentType = null)
        {
            if (await IsHasFile(key) == true) return (true, key);

            try
            {
                PutObjectRequest request = new()
                {
                    BucketName = Bucket,
                    Key = key,
                    ContentType = ContentType ?? GetMimeType(fileStream).Item1,
                    InputStream = fileStream
                };

                PutObjectResponse response = await _client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    return (true, request.Key);

                return (false, null);
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return null;
            }
        }

        public async Task<(bool, string?)?> UploadFile(string key, byte[] fileBytes, string? ContentType = null) => await UploadFile(key, new MemoryStream(fileBytes), ContentType);

        public async Task<string?> GetTempLink(string key)
        {
            try
            {
                GetPreSignedUrlRequest request = new();
                request.BucketName = Bucket;
                request.Key = key;
                request.Expires = DateTime.Now.AddMinutes(30);

                string response = await _client.GetPreSignedURLAsync(request);
                return response;
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return null;
            }
        }

        public async Task<bool?> IsHasFile(string key)
        {
            try
            {
                GetObjectMetadataRequest request = new()
                {
                    BucketName = Bucket,
                    Key = key
                };

                GetObjectMetadataResponse response = await _client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (Amazon.S3.AmazonS3Exception e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return false;
                }
                else
                {
                    Log.Warn(e);
                    return null;
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return null;
            }
        }
    }
}