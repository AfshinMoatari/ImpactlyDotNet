using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using API.Constants;
using API.Models.Reports;
using Microsoft.AspNetCore.Http;

namespace API.Handlers
{
    public class FormFileRequest
    {
        [NotNull] public IFormFile File { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public interface IImageHandler
    {
        public Task<string> UploadImage(string fileId, FormFileRequest request);
        public Task DeleteImage(string key);
        public string GetImagePreSignedURL(string fileId);
        public string GetS3URI(string fileId);

        public string GetObjectUrl(string fileId);
    }

    public class ImageHandler : IImageHandler
    {
        private readonly IAmazonS3 _amazonS3;
        private const int ExpiresHour = 12;

        public ImageHandler(IAmazonS3 amazonS3)
        {
            _amazonS3 = amazonS3;
        }

        public string GetImagePreSignedURL(string fileId) =>
            _amazonS3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = EnvironmentMode.EnvironmentPrefix,
                Key = fileId,
                Expires = DateTime.UtcNow.AddHours(ExpiresHour)
            });

        public string GetS3URI(string fileId)
        {
            return "S3://" + EnvironmentMode.EnvironmentPrefix + "/" + fileId;
        }

        public string GetObjectUrl(string fileId)
        {
            return "https://" + EnvironmentMode.EnvironmentPrefix + 
                   ".s3.eu-central-1.amazonaws.com/" + 
                   fileId;
        }
        
        public async Task<string> UploadImage(string reportId, FormFileRequest request)
        {
            if (request.File.Length == 0)
                return null;
            
            var imageFile = request.File;
            var fileId = Guid.NewGuid().ToString();
            var imageFileLength = imageFile.Length;
            var fileBytes = new byte[imageFileLength];
            var readAsync = await imageFile.OpenReadStream().ReadAsync(fileBytes.AsMemory(0, int.Parse(imageFileLength.ToString())));
            await using var stream = new MemoryStream(fileBytes);

            await _amazonS3.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = EnvironmentMode.EnvironmentPrefix,
                    Key = fileId,
                    InputStream = stream,
                    Metadata =
                    {
                        ["fileName"] = imageFile.FileName,
                        ["fileExtension"] = Path.GetExtension(imageFile.FileName),
                    }
                }
            );

            return fileId;
        }

        public async Task DeleteImage(string key)
        {
            await _amazonS3.DeleteObjectAsync(
                new DeleteObjectRequest()
                {
                    BucketName = EnvironmentMode.EnvironmentPrefix,
                    Key = key
                }
            );
        }
    }
}