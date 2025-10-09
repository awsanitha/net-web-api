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
                ? GetDefaultAllowedMimeTypes()
                : allowedMimeTypes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim()).ToList();
            
            FileSizeLimit = fileSizeLimit <= 0 ? GetDefaultMaxFileSize() : fileSizeLimit;
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

            if (value == null)
            {
                return ValidationResult.Success; // Allow null values, use [Required] for mandatory files
            }

            if (!(value is IFormFile file))
            {
                return new ValidationResult($"{name} must be a valid file.");
            }

            var mimeType = file.ContentType;

            if (!AllowedMimeTypes.Contains(mimeType))
            {
                return new ValidationResult(GetMimeTypeErrorMessage(name, mimeType));
            }

            var length = file.Length;
            var friendlyLength = ByteSize.FromBytes(length).ToString("#.#");
            var friendlyLimit = ByteSize.FromBytes(FileSizeLimit).ToString("#.#");

            return length > FileSizeLimit
                ? new ValidationResult(GetFileSizeErrorMessage(friendlyLength, friendlyLimit))
                : ValidationResult.Success;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the default allowed MIME types.
        /// </summary>
        /// <returns>List of allowed MIME types.</returns>
        private static List<string> GetDefaultAllowedMimeTypes()
        {
            try
            {
                return Settings.Default.AllowedMimeTypes.Cast<string>().ToList();
            }
            catch
            {
                // Fallback to common file types
                return new List<string>
                {
                    "image/jpeg", "image/png", "image/gif", "image/bmp",
                    "application/pdf", "text/plain", "application/msword",
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                };
            }
        }

        /// <summary>
        /// Gets the default maximum file size.
        /// </summary>
        /// <returns>Maximum file size in bytes.</returns>
        private static long GetDefaultMaxFileSize()
        {
            try
            {
                return Settings.Default.MaxAllowedUploadSize;
            }
            catch
            {
                // Fallback to 10MB
                return 10 * 1024 * 1024;
            }
        }

        /// <summary>
        /// Gets the MIME type error message.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <returns>Error message.</returns>
        private static string GetMimeTypeErrorMessage(string fieldName, string mimeType)
        {
            try
            {
                return string.Format(Resources.MimeTypeNotAllowedText, fieldName, mimeType);
            }
            catch
            {
                return $"The file type '{mimeType}' is not allowed for field '{fieldName}'.";
            }
        }

        /// <summary>
        /// Gets the file size error message.
        /// </summary>
        /// <param name="actualSize">The actual size.</param>
        /// <param name="maxSize">The maximum size.</param>
        /// <returns>Error message.</returns>
        private static string GetFileSizeErrorMessage(string actualSize, string maxSize)
        {
            try
            {
                return string.Format(Resources.FileSizeLimitReachedText, actualSize, maxSize);
            }
            catch
            {
                return $"File size {actualSize} exceeds the maximum allowed size of {maxSize}.";
            }
        }

        #endregion
    }
}
