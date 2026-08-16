using System.Collections.Specialized;

namespace Net.Web.Api.Sdk.Properties
{
    /// <summary>
    /// Default application settings.  Replaces the legacy Settings.settings / ApplicationSettingsBase
    /// pattern that is not supported in .NET 10.
    /// </summary>
    internal sealed class Settings
    {
        /// <summary>
        /// Gets the default settings instance.
        /// </summary>
        public static Settings Default { get; } = new Settings();

        private Settings() { }

        /// <summary>Maximum allowed upload size in bytes (default: 2 MB).</summary>
        public long MaxAllowedUploadSize { get; } = 2097152L;

        /// <summary>Allowed MIME types for uploaded files.</summary>
        public StringCollection AllowedMimeTypes { get; } = new StringCollection
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
