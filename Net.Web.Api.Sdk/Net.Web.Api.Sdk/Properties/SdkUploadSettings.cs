namespace Net.Web.Api.Sdk.Properties
{
    /// <summary>
    /// Class SdkUploadSettings.
    /// Options POCO replacing the former Settings.Designer.cs (ApplicationSettingsBase).
    /// </summary>
    public class SdkUploadSettings
    {
        /// <summary>
        /// The configuration section name used for IOptions binding.
        /// </summary>
        public const string SectionName = "SdkUpload";

        /// <summary>
        /// Gets or sets the maximum allowed upload size in bytes.
        /// </summary>
        /// <value>The maximum allowed upload size. Default is 2 MB (2097152 bytes).</value>
        public long MaxAllowedUploadSize { get; set; } = 2097152;

        /// <summary>
        /// Gets or sets the allowed MIME types for file uploads.
        /// </summary>
        /// <value>The allowed MIME types.</value>
        public List<string> AllowedMimeTypes { get; set; } = new()
        {
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.template",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.template",
            "application/vnd.ms-powerpoint",
            "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            "application/vnd.openxmlformats-officedocument.presentationml.template",
            "application/vnd.openxmlformats-officedocument.presentationml.slideshow",
            "image/bmp",
            "image/gif",
            "image/jpeg",
            "image/svg+xml",
            "image/tiff",
            "image/x-icon",
            "image/png",
            "application/json",
            "application/pdf",
            "text/plain",
            "application/xml",
            "text/xml"
        };
    }
}
