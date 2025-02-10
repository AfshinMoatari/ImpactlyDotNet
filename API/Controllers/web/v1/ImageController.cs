using System;
using API.Handlers;
using API.Models;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.web.v1
{
    
    [ApiController]
    [Route("api/web/v1/image")]
    public class ImageController : BaseController
    {
        private readonly IImageHandler _imageHandler;

        public ImageController(IImageHandler imageHandler)
        {
            _imageHandler = imageHandler;
        }
        
        [HttpGet("{fileId}")]
        public ActionResult<string> SignImage(string fileId)
        {
            // Endpoint for signing images with aws public key system
            try
            {
                var signedUrl = _imageHandler.GetImagePreSignedURL(fileId);
                return Ok(signedUrl);
            }
            catch (Exception e)
            {
                return ErrorResponse.NotFound("File Id: " + fileId + " cannot be found.");
            }

        }
    }
}