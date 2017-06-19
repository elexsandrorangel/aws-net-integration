using Amazon;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AWSIntegration
{
    public class GlacierIntegration
    {
        private static string GetAccountId()
        {
            return ConfigurationManager.AppSettings["AWSAccountId"];
        }
        private static IAmazonGlacier GetGlacierClient()
        {
            string accessKey = ConfigurationManager.AppSettings["AWSAccessKey"];
            string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
            return new AmazonGlacierClient(accessKey, secretKey, RegionEndpoint.USEast1);
        }

        #region Vault

        public static bool CreateVault(string vaultName)
        {
            using (var client = GetGlacierClient())
            {
                CreateVaultRequest request = new CreateVaultRequest(GetAccountId(), vaultName);
                CreateVaultResponse response = client.CreateVault(request);
                return response.HttpStatusCode == HttpStatusCode.OK;
            }
        }

        public static Task<CreateVaultResponse> CreateVaultAsync(string vaultName)
        {
            using (var client = GetGlacierClient())
            {
                CreateVaultRequest request = new CreateVaultRequest(GetAccountId(), vaultName);
                return client.CreateVaultAsync(request);
            }
        }

        public static DeleteVaultResponse DeleteVault(string vaultName)
        {
            using (var client = GetGlacierClient())
            {
                DeleteVaultRequest request = new DeleteVaultRequest(GetAccountId(), vaultName);

                return client.DeleteVault(request);
            }
        }

        public static Task<DeleteVaultResponse> DeleteVaultAsync(string vaultName)
        {
            using (var client = GetGlacierClient())
            {
                DeleteVaultRequest request = new DeleteVaultRequest(GetAccountId(), vaultName);

                return client.DeleteVaultAsync(request);
            }
        }

        public static DescribeVaultResponse DescribeVault(string vaultName)
        {
            using (var client = GetGlacierClient())
            {
                DescribeVaultRequest request = new DescribeVaultRequest(GetAccountId(), vaultName);
                return client.DescribeVault(request);
            }
        }

        public static Task<DescribeVaultResponse> DescribeVaultAsync(string vaultName)
        {
            using (var client = GetGlacierClient())
            {
                DescribeVaultRequest request = new DescribeVaultRequest(GetAccountId(), vaultName);
                return client.DescribeVaultAsync(request);
            }
        }

        public static List<DescribeVaultOutput> ListVaults()
        {
            using (var client = GetGlacierClient())
            {
                ListVaultsRequest request = new ListVaultsRequest(GetAccountId(), "", 100);
                ListVaultsResponse response = client.ListVaults(request);
                return response.VaultList;
            }
        }

        public static async Task<List<DescribeVaultOutput>> ListVaultsAsync()
        {
            using (var client = GetGlacierClient())
            {
                ListVaultsRequest request = new ListVaultsRequest(GetAccountId(), "", 100);
                ListVaultsResponse response = await client.ListVaultsAsync(request);
                return response.VaultList;
            }
        }

        #endregion Vault

        #region Jobs

        public static DescribeJobResponse DescribeJob(string vaultName, string jobId)
        {
            using (var client = GetGlacierClient())
            {
                DescribeJobRequest request = new DescribeJobRequest(GetAccountId(), vaultName, jobId);
                return client.DescribeJob(request);
            }
        }

        public static Task<DescribeJobResponse> DescribeJobAsync(string vaultName, string jobId)
        {
            using (var client = GetGlacierClient())
            {
                DescribeJobRequest request = new DescribeJobRequest(GetAccountId(), vaultName, jobId);
                return client.DescribeJobAsync(request);
            }
        }

        public static InitiateJobResponse InitiateJob()
        {
            throw new NotImplementedException();
        }

        #endregion Jobs

        #region Files

        public static DeleteArchiveResponse DeleteArchive(string vaultName, string archiveId)
        {
            using (var client = GetGlacierClient())
            {
                DeleteArchiveRequest request = new DeleteArchiveRequest(GetAccountId(), vaultName, archiveId);

                return client.DeleteArchive(request);
            }
        }

        public static Task<DeleteArchiveResponse> DeleteArchiveAsync(string vaultName, string archiveId)
        {
            using (var client = GetGlacierClient())
            {
                DeleteArchiveRequest request = new DeleteArchiveRequest(GetAccountId(), vaultName, archiveId);
                return client.DeleteArchiveAsync(request);
            }
        }


        public static UploadArchiveResponse UploadArchive(string vaultName, string archiveDescription, string checksum, Stream body)
        {
            using (var client = GetGlacierClient())
            {
                UploadArchiveRequest request = new UploadArchiveRequest(GetAccountId(), vaultName, archiveDescription, checksum, body);

                return client.UploadArchive(request);
            }
        }

        public static Task<UploadArchiveResponse> UploadArchiveAsync(string vaultName, string archiveDescription, string checksum, Stream body)
        {
            using (var client = GetGlacierClient())
            {
                UploadArchiveRequest request = new UploadArchiveRequest(GetAccountId(), vaultName, archiveDescription, checksum, body);

                return client.UploadArchiveAsync(request);
            }
        }

        #endregion Files
    }
}
