using ByteSizeLib;
using Microsoft.AspNetCore.Http;
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
        public IList<string> AllowedMimeTypes { get; }

        /// <summary>
        /// Gets the file size limit.
        /// </summary>
        /// <value>The file size limit.</value>
        public long FileSizeLimit { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileAttribute"/> class.
        /// </summary>
        /// <param name="allowedMimeTypes">The allowed MIME types.</param>
        /// <param name="fileSizeLimit">The file size limit.</param>
        public UploadFileAttribute(string allowedMimeTypes = null, long fileSizeLimit = 0)
        {
            AllowedMimeTypes = string.IsNullOrEmpty(allowedMimeTypes)
                ? Settings.Default.AllowedMimeTypes.Cast<string>().ToList()
                : allowedMimeTypes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim()).ToList();

            FileSizeLimit = fileSizeLimit <= 0 ? Settings.Default.MaxAllowedUploadSize : fileSizeLimit;
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
            var name = string.IsNullOrEmpty(validationContext.DisplayName)
                ? validationContext.MemberName
                : validationContext.DisplayName;

            // A missing value is the concern of [Required], not of this attribute.
            if (value == null)
            {
                return ValidationResult.Success;
            }

            switch (value)
            {
                case IFormFile file:
                    return ValidateFile(name, file.ContentType, file.Length);

                case IEnumerable<IFormFile> files:
                    foreach (var file in files)
                    {
                        var result = ValidateFile(name, file?.ContentType, file?.Length ?? 0);

                        if (result != ValidationResult.Success)
                        {
                            return result;
                        }
                    }

                    return ValidationResult.Success;

                case byte[] buffer:
                    // No MIME type is carried by a raw buffer, the size limit still applies.
                    return ValidateSize(buffer.LongLength);

                default:
                    // The legacy attribute cast the value to HttpFile and threw on anything else.
                    // Never pass an unrecognized value silently: that would skip MIME and size checks.
                    return new ValidationResult(string.Format(Resources.MimeTypeNotAllowedText, name, value.GetType().Name));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates the MIME type and the size of an uploaded file.
        /// </summary>
        /// <param name="name">The display name of the validated member.</param>
        /// <param name="mimeType">The MIME type of the file.</param>
        /// <param name="length">The length of the file, in bytes.</param>
        /// <returns>An instance of the <see cref="ValidationResult" /> class.</returns>
        private ValidationResult ValidateFile(string name, string mimeType, long length)
        {
            return !AllowedMimeTypes.Contains(mimeType)
                ? new ValidationResult(string.Format(Resources.MimeTypeNotAllowedText, name, mimeType))
                : ValidateSize(length);
        }

        /// <summary>
        /// Validates the size of an uploaded file against the configured limit.
        /// </summary>
        /// <param name="length">The length of the file, in bytes.</param>
        /// <returns>An instance of the <see cref="ValidationResult" /> class.</returns>
        private ValidationResult ValidateSize(long length)
        {
            var friendlyLength = ByteSize.FromBytes(length).ToString("#.#");
            var friendlyLimit = ByteSize.FromBytes(FileSizeLimit).ToString("#.#");

            return length > FileSizeLimit
                ? new ValidationResult(string.Format(Resources.FileSizeLimitReachedText, friendlyLength, friendlyLimit))
                : ValidationResult.Success;
        }

        #endregion
    }
}
