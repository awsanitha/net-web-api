using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Attributes.Validations;

namespace Net.Web.Api.Sdk.Web.Examples.Models
{
    /// <summary>
    /// File upload request model.
    /// </summary>
    public class UploadRequest
    {
        /// <summary>
        /// File to upload.
        /// </summary>
        [Required]
        [UploadFile]
        public IFormFile FileInformation { get; set; }
    }
}
