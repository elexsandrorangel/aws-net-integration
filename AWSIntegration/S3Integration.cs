using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;

namespace AWSIntegration
{
    public class S3Integration
    {
        private static IAmazonS3 GetAmazonS3ClientInstance()
        {
            string accessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
            return new AmazonS3Client(accessKey, secretKey, RegionEndpoint.USEast1);
        }

        public static string DefaultRegion
        {
            get
            {
                return ConfigurationManager.AppSettings["AWSRegion"];
            }
        }

        #region Buckets
        /// <summary>
        /// Returns a list of all buckets owned by the authenticated sender of the request.
        /// </summary>
        /// <returns>List of buckets</returns>
        public static List<S3Bucket> ListBuckets()
        {
            using (var client = GetAmazonS3ClientInstance())
            {
                ListBucketsResponse response = client.ListBuckets();
                List<S3Bucket> buckets = response.Buckets;

                return buckets;
            }
        }

        /// <summary>
        /// Returns a list of all buckets names owned by the authenticated sender of the request.
        /// </summary>
        /// <returns>Names of buckets</returns>
        public static List<string> ListBucketNames()
        {
            List<string> bucketsNames = ListBuckets().Select(b => b.BucketName).ToList();

            return bucketsNames;
        }

        /// <summary>
        /// Create a new bucket if not exists
        /// </summary>
        /// <param name="bucketName">Name</param>
        public static void CreateBucketIfNotExists(string bucketName)
        {
            if (!string.IsNullOrEmpty(bucketName))
            {
                bucketName = bucketName.ToLower().Trim();

                List<S3Bucket> myBuckets = ListBuckets();

                if (myBuckets != null)
                {
                    bool exist = myBuckets.Any(b => b.BucketName.ToLower().Equals(bucketName));

                    if (!exist)
                    {
                        CreateBucket(bucketName);
                    }
                }
            }
        }

        /// <summary>
        /// Create a new bucket
        /// </summary>
        /// <param name="bucketName">Name</param>
        /// <returns></returns>
        public static bool CreateBucket(string bucketName)
        {
            if (string.IsNullOrEmpty(bucketName))
            {
                return false;
            }

            using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
            {
                PutBucketRequest putBucketRequest = new PutBucketRequest();
                putBucketRequest.BucketName = bucketName;
                putBucketRequest.BucketRegion = S3Region.US;
                PutBucketResponse putBucketResponse = s3Client.PutBucket(putBucketRequest);
                return putBucketResponse.HttpStatusCode == HttpStatusCode.OK;
            }
        }

        /// <summary>
        /// Delete a bucket by name
        /// </summary>
        /// <param name="bucketName">Bucket name</param>
        public static void DeleteBucket(string bucketName)
        {
            using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
            {
                DeleteBucketRequest request = new DeleteBucketRequest();
                request.BucketName = bucketName;
                request.BucketRegion = new S3Region(DefaultRegion);

                s3Client.DeleteBucket(bucketName);
            }
        }

        #endregion Buckets

        #region Objects Management

        /// <summary>
        /// Create a new file in the bucket
        /// </summary>
        /// <param name="fileContent">File's content</param>
        /// <param name="fileName">Filename</param>
        /// <param name="bucketName">Bucket</param>
        /// <param name="metadata">Dictionary with metadata</param>
        /// <returns></returns>
        public static bool CreateFile(string fileContent, string fileName, string bucketName, Dictionary<string, string> metadata)
        {
            using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
            {
                PutObjectRequest request = new PutObjectRequest();
                request.ContentBody = fileContent;
                request.BucketName = bucketName;
                request.Key = fileName;

                if (metadata != null && metadata.Any())
                {
                    foreach (var item in metadata)
                    {
                        request.Metadata.Add(item.Key, item.Value);
                    }
                }

                PutObjectResponse response = s3Client.PutObject(request);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }

        public static bool UploadFile(string bucketName, string fileName, byte[] fileBytes)
        {
            return UploadFile(bucketName, fileName, fileBytes, null);
        }

        public static bool UploadFile(string bucketName, string fileName, byte[] fileBytes, Dictionary<string, string> metadata)
        {
            using (MemoryStream ms = new MemoryStream(fileBytes))
            {
                return UploadFile(bucketName, fileName, ms, metadata);
            }
        }

        public static bool UploadFile(string bucketName, string path, Stream inputStream, Dictionary<string, string> metadata)
        {
            using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
            {
                CreateBucketIfNotExists(bucketName);

                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = bucketName;
                request.Key = path;
                request.InputStream = inputStream;

                if (metadata != null && metadata.Any())
                {
                    // Adiciona os metadados e headers HTTP no objeto
                    foreach (var meta in metadata)
                    {
                        request.Metadata.Add(meta.Key, meta.Value);
                    }
                }

                PutObjectResponse response = s3Client.PutObject(request);

                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }

        public static byte[] GetFile(string bucketName, string keyName)
        {
            using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                using (GetObjectResponse response = s3Client.GetObject(request))
                {
                    byte[] buffer = new byte[16 * 1024];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;

                        while ((read = response.ResponseStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            ms.Write(buffer, 0, read);
                        }

                        return ms.ToArray();
                    }
                }
            }
        }

        public static void DeletingAnObject(string bucketName, string keyName)
        {
            try
            {
                using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
                {
                    DeleteObjectRequest request = new DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = keyName
                    };

                    s3Client.DeleteObject(request);
                }
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    throw new Exception("Please check the provided AWS Credentials.");
                }
                throw new Exception(String.Format("An error occurred with the message '{0}' when deleting an object", amazonS3Exception.Message));
            }
        }

        public static List<S3Object> ListObjects(string bucketName, string prefix = null, int? maxEntries = null)
        {
            using (IAmazonS3 s3Client = GetAmazonS3ClientInstance())
            {
                ListObjectsRequest request = new ListObjectsRequest();
                request.BucketName = bucketName;

                if (!string.IsNullOrEmpty(prefix))
                {
                    request.Prefix = prefix;
                }

                if (maxEntries.HasValue)
                {
                    request.MaxKeys = maxEntries.Value;
                }

                ListObjectsResponse response = s3Client.ListObjects(request);

                return response.S3Objects;
            }
        }

        #endregion Objects Management
    }
}