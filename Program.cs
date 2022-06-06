using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace az204_blob
{
    class Program
    {
        static string localFileName = "wtfile" + Guid.NewGuid().ToString() + ".txt";
        static string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=spinatose;AccountKey=dM8k+yZyknnp09bER7Kw+KxWKT+VFjg7g2QEwVmPIBibNu82KwifhRarHyAkElm+779j7KpKUTZT+AStqN2iuQ==;EndpointSuffix=core.windows.net";
        //Create a unique name for the container
        static string containerName = "wtblob" + Guid.NewGuid().ToString();

        public static void Main()
        {
            Console.WriteLine("Azure Blob Storage exercise\n");

            // Run the examples asynchronously, wait for the results before proceeding
            CreateContainerWithBlobAsync().GetAwaiter().GetResult();

            ListBlobsInContainerAsync().GetAwaiter().GetResult();

            DownloadBlobAsync().GetAwaiter().GetResult();

            DeleteContainerAndBlobFileAsync().GetAwaiter().GetResult();

            Console.WriteLine("Press enter to exit the sample application.");
            Console.ReadLine();
        }

        private static async Task ListBlobsInContainerAsync() 
        {
            // get blob client in order to get container
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // List blobs in the container
            Console.WriteLine("Listing blobs...");
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }

            Console.WriteLine("\nYou can also verify by looking inside the " + 
                    "container in the portal." +
                    "\nNext the blob will be downloaded with an altered file name.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();           
        }

        private static async Task DeleteContainerAndBlobFileAsync() 
        {
            // get blob client in order to get container
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            string localPath = "./data/";
            string localFilePath = Path.Combine(localPath, localFileName);          
            string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

            // Delete the container and clean up local files created
            Console.WriteLine("\n\nDeleting blob container...");
            await containerClient.DeleteAsync();

            Console.WriteLine("Deleting the local source and downloaded files...");
            File.Delete(localFilePath);
            File.Delete(downloadFilePath);

            Console.WriteLine("Finished cleaning up.");
        }

        private static async Task DownloadBlobAsync()
        {   
            // Create a client that can authenticate with a connection string
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);
        
            // Download the blob to a local file
            string localPath = "./data/";
            string localFilePath = Path.Combine(localPath, localFileName);
            // Append the string "DOWNLOADED" before the .txt extension 
            string downloadFilePath = localFilePath.Replace(".txt", "DOWNLOADED.txt");

            Console.WriteLine("\nDownloading blob to\n\t{0}\n", downloadFilePath);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(localFileName);

            // Download the blob's contents and save it to a file
            BlobDownloadInfo download = await blobClient.DownloadAsync();

            using (FileStream downloadFileStream = File.OpenWrite(downloadFilePath))
            {
                await download.Content.CopyToAsync(downloadFileStream);
                downloadFileStream.Close();
            }
            Console.WriteLine("\nLocate the local file to verify it was downloaded.");
            Console.WriteLine("The next step is to delete the container and local files.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();
        }

        private static async Task CreateContainerWithBlobAsync()
        {
            // Create a client that can authenticate with a connection string
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageConnectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);
            Console.WriteLine("A container named '" + containerName + "' has been created. " +
                "\nTake a minute and verify in the portal." + 
                "\nNext a file will be created and uploaded to the container.");

            await ReadContainerPropertiesAsync(containerClient);

            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "./data/";
            string localFilePath = Path.Combine(localPath, localFileName);

            // Write text to the file
            await File.WriteAllTextAsync(localFilePath, "hola, mundo!");

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(localFileName);

            Console.WriteLine("Uploading to Blob storage as blob:\n\t {0}\n", blobClient.Uri);

            // Open the file and upload its data
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();

            Console.WriteLine("\nThe file was uploaded. We'll verify by listing" + 
                    " the blobs next.");
            Console.WriteLine("Press 'Enter' to continue.");
            Console.ReadLine();
        }

        private static async Task ReadContainerPropertiesAsync(BlobContainerClient container)
        {
            try
            {
                // Fetch some container properties and write out their values.
                var properties = await container.GetPropertiesAsync();
                Console.WriteLine($"Properties for container {container.Uri}");
                Console.WriteLine($"Public access level: {properties.Value.PublicAccess}");
                Console.WriteLine($"Last modified time in UTC: {properties.Value.LastModified}");
            }
            catch (RequestFailedException e)
            {
                Console.WriteLine($"HTTP error code {e.Status}: {e.ErrorCode}");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }
    }
}