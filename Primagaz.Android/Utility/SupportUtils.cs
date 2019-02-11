using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Transfer;
using Java.Util.Zip;
using Primagaz.Standard;

namespace Primagaz.Android
{
    public static class SupportUtils
    {
        public static Task UploadDatabase()
        {
            var credentials = new CognitoAWSCredentials("eu-west-1:ff2a32c4-c7a0-4340-9c41-1cddf407c367", RegionEndpoint.EUWest1);

            var s3Client = new AmazonS3Client(credentials, RegionEndpoint.EUWest1);
            var transferUtility = new TransferUtility(s3Client);

            var file = PrepareFile();
            return transferUtility.UploadAsync(file, $"primagaz-databases");
        }

        /// <summary>
        /// Prepare the file
        /// </summary>
        /// <returns>The file.</returns>
        public static string PrepareFile()
        {
            var id = Guid.NewGuid().ToString();

            using (var repo = new Repository())
            {
                var profile = repo.Profiles.First();
                var date = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

                var databaseFile = Repository.GetDatabasePath();
                var zipFile = $"/data/user/0/se.primagaz.cylinder/files/{profile.SubscriberID}_{date}.zip";

                using (var fileStream = new FileStream(zipFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    using (var zipStream = new ZipOutputStream(fileStream))
                    {
                        var zipEntry = new ZipEntry(databaseFile);
                        zipStream.PutNextEntry(zipEntry);

                        var fileContent = File.ReadAllBytes(databaseFile);
                        zipStream.Write(fileContent);

                        zipStream.CloseEntry();

                        zipStream.Close();
                    }

                    fileStream.Close();
                }

                return zipFile;

            }


        }
    }
}
