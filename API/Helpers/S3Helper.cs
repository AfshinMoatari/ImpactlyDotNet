using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime.Internal;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using API.Constants;
using API.Models.Views.Report;
using API.Repositories;
using API.Services.External;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Nest;
using Newtonsoft.Json;

namespace API.Helpers;

public interface IS3Helper
{
    /// <summary>
    /// Check if the folder with key name is exist in the S3 bucket
    /// </summary>
    /// <returns>true if folder exist.</returns>
    public Task<bool> IfFolderExists(string key, string S3Bucket);
    /// <summary>
    /// check if the file with file name is exist in the S3 bucket
    /// </summary>
    /// <returns>true if exist.</returns>
    public Task<bool> IfFileExists(string fileName, string versionId, string S3Bucket);
    /// <summary>
    /// create file in S3 bucket with fileinfo object on specic path
    /// </summary>
    /// <returns>Presigned url of the file.</returns>
    public Task<string> CreateS3File(string subDirectoryInBucket, FileInfo fileInfo, string S3Bucket);
    /// <summary>
    /// Create a folder with specif path on a bucket.
    /// </summary>
    /// <returns>nothing.</returns>
    public Task CreateS3Directory(string folderName, string S3Bucket);
    /// <summary>
    /// Read and find the file on specfc bucket and save it to local path.
    /// </summary>
    /// <returns>true if sucessfull.</returns>
    public Task<bool> ReadS3AndSave(string fileName, string saveToFolder, string S3Bucket);
    /// <summary>
    /// Get the resigned URL based on key.
    /// </summary>
    /// <returns>presigned url.</returns>
    public string GetFilePreSignedUrl(string key, string S3Bucket, string fileFormat = null);
    /// <summary>
    /// Find and remove an object with name from S3bucket.
    /// </summary>
    /// <returns>Status of deletion.</returns>
    public Task<HttpStatusCode> DeleteS3File(string fileName, string versionId, string S3Bucket, string fileFormat = null);
}

/// <summary>
/// S3 helper class for managing all the S3 operations.
/// </summary>
public class S3Helper : IS3Helper
{
    private readonly IAmazonS3 _amazonS3;
    private readonly int ExpiresHour = 12;

    /// <summary>
    /// Initializes a new instance of the <see cref="S3Helper"/> class.
    /// </summary>
    /// <param name="amazonS3Client">The _amazonS3.</param>
    public S3Helper(IAmazonS3 amazonS3)
    {
        _amazonS3 = amazonS3;
    }

    private string GetBucketName(string s3Buckets)
    {
        var bucketExtension = s3Buckets.ToString();

        if (EnvironmentMode.IsProduction)
        {
            return $"impactly-production-{bucketExtension}";
        }
        if (EnvironmentMode.IsStaging)
        {
            return $"impactly-staging-{bucketExtension}";
        }
        return $"impactly-staging-{bucketExtension}";
    }
    
    public async Task<bool> IfFolderExists(string key, string S3Bucket)
    {
        ListObjectsResponse response = null;
        try
        {
            var request = new ListObjectsRequest
            {
                BucketName = GetBucketName(S3Bucket),
                Prefix = key
            };
            response = await _amazonS3.ListObjectsAsync(request);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return response is { S3Objects.Count: > 0 };
    }

    public async Task<bool> IfFileExists(string fileName, string versionId, string S3Bucket)
    {
        try
        {
            var request = new GetObjectMetadataRequest()
            {
                BucketName = GetBucketName(S3Bucket),
                Key = fileName,
                VersionId = !string.IsNullOrEmpty(versionId) ? versionId : null
            };

            var response = await _amazonS3.GetObjectMetadataAsync(request);

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"IsFileExists: Error during checking if file exists in s3 bucket: {JsonConvert.SerializeObject(ex)}");
            return false;
        }
        
    }

    public async Task<string> CreateS3File(string subDirectoryInBucket, FileInfo fileInfo, string S3Bucket)
    {
        var utility = new TransferUtility(_amazonS3);  
        var request = new TransferUtilityUploadRequest();
        
        if (string.IsNullOrEmpty(subDirectoryInBucket))  
        {  
            request.BucketName = GetBucketName(S3Bucket); //no subdirectory just bucket name  
        }  
        else  
        {   // subdirectory and bucket name  
            if (!await IfFolderExists(subDirectoryInBucket, S3Bucket))
            {
                await CreateS3Directory(subDirectoryInBucket, S3Bucket);
            }

            request.BucketName = GetBucketName(S3Bucket) + @"/" + subDirectoryInBucket;  
        }  
        request.Key = fileInfo.Name; //file name up in S3
        request.FilePath = fileInfo.DirectoryName + "/" + fileInfo.Name;
        await utility.UploadAsync(request);
        return GetFilePreSignedUrl(request.Key, S3Bucket);
    }

    public async Task CreateS3Directory(string folderName, string S3Bucket)
    {
        try
        {
            var request = new PutObjectRequest()  
            {  
                BucketName = GetBucketName(S3Bucket),  
                Key = folderName  
            };  
  
            await _amazonS3.PutObjectAsync(request); 
        }
        catch (Exception e)
        {
            Console.WriteLine(e.StackTrace);
        }
        
    }

    public async Task<bool> ReadS3AndSave(string fileName, string saveToFolder, string S3Bucket)
    {
        var request = new GetObjectRequest
        {
            BucketName = GetBucketName(S3Bucket),
            Key = fileName,
        };

        // Issue request and remember to dispose of the response
        using var response = await _amazonS3.GetObjectAsync(request);
        try
        {
            // Save object to local file
            var path = Path.Combine(saveToFolder, fileName);
            await response.WriteResponseStreamToFileAsync(path, true, CancellationToken.None);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            Console.WriteLine($"Error saving {fileName}: {ex.Message}");
            return false;
        }
    }

    public string GetFilePreSignedUrl(string key, string S3Bucket, string fileFormat = null)
    {
        return _amazonS3.GetPreSignedURL(new GetPreSignedUrlRequest()
        {
            BucketName = GetBucketName(S3Bucket),
            Key = key + fileFormat,
            Expires = DateTime.UtcNow.AddHours(ExpiresHour)
        });
    }

    public async Task<HttpStatusCode> DeleteS3File(string fileName, string versionId, string S3Bucket, string fileFormat = null)
    {
        var ifExist = await IfFileExists(fileName + fileFormat, versionId, S3Bucket);

        if (ifExist)
        {
            try
            {
                var request = new DeleteObjectRequest()
                {
                    BucketName = GetBucketName(S3Bucket),
                    Key = fileName + fileFormat
                };

                var response = await _amazonS3.DeleteObjectAsync(request);
                return response.HttpStatusCode;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }
        return HttpStatusCode.BadRequest;
        
    }
}
