using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Attributes.Validations;

namespace Net.Web.Api.Sdk.Web.Examples.Models
{
    /// <summary>
    /// Request model for file uploads.
    /// </summary>
    public class UploadRequest
    {
        /// <summary>
        /// The file to upload.
        /// </summary>
        [Required]
        [UploadFile]
        public IFormFile FileInformation { get; set; }
    }
}
