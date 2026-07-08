using System.Collections.Generic;

namespace Net.Web.Api.Sdk.Properties
{
    /// <summary>
    /// Settings for the SDK - replaces the old ApplicationSettingsBase approach.
    /// </summary>
    internal static class Settings
    {
        private static SettingsDefaults _default = new SettingsDefaults();

        public static SettingsDefaults Default => _default;

        internal class SettingsDefaults
        {
            public long MaxAllowedUploadSize { get; } = 2097152;

            public IList<string> AllowedMimeTypes { get; } = new List<string>
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
}
