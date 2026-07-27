using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Net.Web.Api.Sdk.Web.Examples.Models
{
    /// <summary>
    /// Class UploadRequest.
    /// </summary>
    public class UploadRequest
    {
        #region Public Properties

        /// <summary>
        /// File to upload.
        /// </summary>
        /// <value>The file information.</value>
        [Required]
        public IFormFile FileInformation { get; set; }

        #endregion
    }
}
