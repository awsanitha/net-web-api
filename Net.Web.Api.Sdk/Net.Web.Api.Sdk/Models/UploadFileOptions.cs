using System.Collections.Generic;

namespace Net.Web.Api.Sdk.Models
{
    /// <summary>
    /// Class UploadFileOptions.
    /// Options for file upload validation, replacing Settings.Default usage.
    /// </summary>
    public class UploadFileOptions
    {
        /// <summary>
        /// Gets or sets the allowed MIME types.
        /// </summary>
        /// <value>The allowed MIME types.</value>
        public IList<string> AllowedMimeTypes { get; set; } = new List<string> { "application/octet-stream" };

        /// <summary>
        /// Gets or sets the maximum allowed upload size in bytes.
        /// </summary>
        /// <value>The maximum allowed upload size.</value>
        public long MaxAllowedUploadSize { get; set; } = 10 * 1024 * 1024; // 10 MB
    }
}
