using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Net.Web.Api.Sdk.Properties
{
    /// <summary>
    /// Class Settings.
    /// Replaces the legacy <c>ApplicationSettingsBase</c> designer settings. Values keep the same
    /// defaults as the former <c>app.config</c> application settings and can be overridden from
    /// configuration through the <c>NetWebApiSdk</c> section of <c>appsettings.json</c>.
    /// </summary>
    internal sealed class Settings
    {
        #region Private Constants

        /// <summary>
        /// The configuration section name
        /// </summary>
        private const string SECTION_NAME = "NetWebApiSdk";

        #endregion

        #region Singleton

        /// <summary>
        /// The default instance
        /// </summary>
        private static Settings _default = new Settings();

        /// <summary>
        /// Gets the default settings instance.
        /// </summary>
        /// <value>The default.</value>
        public static Settings Default => _default;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the maximum allowed upload size in bytes.
        /// </summary>
        /// <value>The maximum allowed upload size.</value>
        public long MaxAllowedUploadSize { get; private set; } = 2097152;

        /// <summary>
        /// Gets the allowed MIME types.
        /// </summary>
        /// <value>The allowed MIME types.</value>
        public IList<string> AllowedMimeTypes { get; private set; } = new List<string>
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

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the settings from the application configuration, keeping the built in defaults for
        /// every value the configuration does not supply.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        internal static void Bind(IConfiguration configuration)
        {
            var section = configuration?.GetSection(SECTION_NAME);

            if (section == null || !section.Exists())
            {
                return;
            }

            var settings = new Settings();
            var maxAllowedUploadSize = section.GetValue<long?>(nameof(MaxAllowedUploadSize));

            if (maxAllowedUploadSize.HasValue && maxAllowedUploadSize.Value > 0)
            {
                settings.MaxAllowedUploadSize = maxAllowedUploadSize.Value;
            }

            var allowedMimeTypes = section.GetSection(nameof(AllowedMimeTypes)).Get<List<string>>();

            if (allowedMimeTypes != null && allowedMimeTypes.Count > 0)
            {
                settings.AllowedMimeTypes = allowedMimeTypes;
            }

            _default = settings;
        }

        #endregion
    }
}
