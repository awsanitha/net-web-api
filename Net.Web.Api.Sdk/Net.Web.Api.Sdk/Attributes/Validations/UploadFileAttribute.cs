using ByteSizeLib;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Net.Web.Api.Sdk.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    /// <summary>
    /// Class UploadFileAttribute.
    /// Implements the <see cref="ValidationAttribute" />
    /// </summary>
    /// <seealso cref="ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class UploadFileAttribute : ValidationAttribute
    {
        #region Properties

        /// <summary>
        /// Gets the allowed MIME types.
        /// </summary>
        /// <value>The allowed MIME types.</value>
        public IList<string> AllowedMimeTypes { get; private set; }

        /// <summary>
        /// Gets the file size limit.
        /// </summary>
        /// <value>The file size limit.</value>
        public long FileSizeLimit { get; private set; }

        #endregion

        #region Fields

        private readonly string _allowedMimeTypesParam;
        private readonly long _fileSizeLimitParam;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileAttribute"/> class.
        /// </summary>
        /// <param name="allowedMimeTypes">The allowed MIME types.</param>
        /// <param name="fileSizeLimit">The file size limit.</param>
        public UploadFileAttribute(string allowedMimeTypes = null, long fileSizeLimit = 0)
        {
            _allowedMimeTypesParam = allowedMimeTypes;
            _fileSizeLimitParam = fileSizeLimit;
        }

        #endregion

        #region ValidationAttribute Overrides

        /// <summary>
        /// Validates the specified value with respect to the current validation attribute.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>An instance of the <see cref="T:System.ComponentModel.DataAnnotations.ValidationResult" /> class.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Resolve settings from DI via ValidationContext
            var settingsOptions = validationContext.GetService(typeof(IOptions<SdkUploadSettings>)) as IOptions<SdkUploadSettings>;
            var settings = settingsOptions?.Value ?? new SdkUploadSettings();

            AllowedMimeTypes = string.IsNullOrEmpty(_allowedMimeTypesParam)
                ? settings.AllowedMimeTypes
                : _allowedMimeTypesParam.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim()).ToList();

            FileSizeLimit = _fileSizeLimitParam <= 0 ? settings.MaxAllowedUploadSize : _fileSizeLimitParam;

            var name = string.IsNullOrEmpty(validationContext.DisplayName)
                ? validationContext.MemberName
                : validationContext.DisplayName;
            var formFile = (IFormFile)value;
            var mimeType = formFile.ContentType;

            if (!AllowedMimeTypes.Contains(mimeType))
            {
                return new ValidationResult(string.Format(Resources.MimeTypeNotAllowedText, name, mimeType));
            }

            var length = formFile.Length;
            var friendlyLength = ByteSize.FromBytes(length).ToString("#.#");
            var friendlyLimit = ByteSize.FromBytes(FileSizeLimit).ToString("#.#");

            return length > FileSizeLimit
                ? new ValidationResult(string.Format(Resources.FileSizeLimitReachedText, friendlyLength, friendlyLimit))
                : ValidationResult.Success;
        }

        #endregion
    }
}
