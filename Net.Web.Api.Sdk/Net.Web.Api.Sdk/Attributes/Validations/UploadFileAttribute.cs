using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ByteSizeLib;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Properties;

namespace Net.Web.Api.Sdk.Attributes.Validations
{
    /// <summary>
    /// Validates that an uploaded <see cref="IFormFile"/> satisfies the allowed MIME types and size limits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UploadFileAttribute : ValidationAttribute
    {
        #region Properties

        /// <summary>Gets the allowed MIME types.</summary>
        public IList<string> AllowedMimeTypes { get; }

        /// <summary>Gets the maximum file size in bytes.</summary>
        public long FileSizeLimit { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UploadFileAttribute"/>.
        /// </summary>
        /// <param name="allowedMimeTypes">Comma-separated list of allowed MIME types, or <c>null</c> to use defaults.</param>
        /// <param name="fileSizeLimit">Maximum file size in bytes, or 0 to use the default (2 MB).</param>
        public UploadFileAttribute(string allowedMimeTypes = null, long fileSizeLimit = 0)
        {
            AllowedMimeTypes = string.IsNullOrEmpty(allowedMimeTypes)
                ? Settings.Default.AllowedMimeTypes.Cast<string>().ToList()
                : allowedMimeTypes
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .ToList();

            FileSizeLimit = fileSizeLimit <= 0 ? Settings.Default.MaxAllowedUploadSize : fileSizeLimit;
        }

        #endregion

        #region ValidationAttribute

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var name = string.IsNullOrEmpty(validationContext.DisplayName)
                ? validationContext.MemberName
                : validationContext.DisplayName;

            if (value == null)
                return ValidationResult.Success;

            if (value is not IFormFile file)
                return new ValidationResult($"The field '{name}' must be a file.");

            var mimeType = file.ContentType;

            if (!AllowedMimeTypes.Contains(mimeType))
            {
                return new ValidationResult(
                    string.Format(Resources.MimeTypeNotAllowedText, name, mimeType));
            }

            var length = file.Length;
            var friendlyLength = ByteSize.FromBytes(length).ToString("#.#");
            var friendlyLimit = ByteSize.FromBytes(FileSizeLimit).ToString("#.#");

            return length > FileSizeLimit
                ? new ValidationResult(
                    string.Format(Resources.FileSizeLimitReachedText, friendlyLength, friendlyLimit))
                : ValidationResult.Success;
        }

        #endregion
    }
}
