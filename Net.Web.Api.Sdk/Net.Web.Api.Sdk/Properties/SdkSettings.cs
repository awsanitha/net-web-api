using System.Collections.Generic;

namespace Net.Web.Api.Sdk.Properties
{
    /// <summary>
    /// SDK settings - replaces ApplicationSettingsBase for .NET Core compatibility.
    /// </summary>
    internal static class Settings
    {
        public static long MaxAllowedUploadSize => 2097152;

        public static IList<string> AllowedMimeTypes => new List<string>
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
