using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Interfaces.File;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Net.Web.Api.Sdk.Implementations.File
{
    /// <summary>
    /// Class FileService.
    /// Implements the <see cref="IFileService" />
    /// </summary>
    /// <seealso cref="IFileService" />
    public class FileService : IFileService
    {
        #region Constants

        /// <summary>
        /// The upload directory name
        /// </summary>
        private const string UPLOAD_DIRECTORY_NAME = "Upload";

        #endregion

        #region Private Fields

        /// <summary>
        /// The HTTP context accessor
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public FileService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        #endregion

        #region IFileService Implementations

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Uri.</returns>
        public Uri UploadFile(byte[] content, string fileName)
        {
            var contentRootPath = AppContext.BaseDirectory;
            var destinationDirectory = Path.Combine(contentRootPath, UPLOAD_DIRECTORY_NAME);

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            var destinationFile = GetUniqueFileName(Path.Combine(destinationDirectory, fileName));

            System.IO.File.WriteAllBytes(destinationFile, content);

            var httpContext = _httpContextAccessor.HttpContext;
            var request = httpContext?.Request;

            var baseUrl = request != null
                ? $"{request.Scheme}://{request.Host}"
                : string.Empty;

            return new Uri($"{baseUrl}/{UPLOAD_DIRECTORY_NAME}/{Path.GetFileName(destinationFile)}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the name of the unique file.
        /// </summary>
        /// <param name="fullFileName">Full name of the file.</param>
        /// <returns>System.String.</returns>
        private static string GetUniqueFileName(string fullFileName)
        {
            if (!System.IO.File.Exists(fullFileName))
            {
                return fullFileName;
            }

            var folder = Path.GetDirectoryName(fullFileName);

            if (folder == null)
            {
                return fullFileName;
            }

            var filename = Path.GetFileNameWithoutExtension(fullFileName);
            var extension = Path.GetExtension(fullFileName);
            var number = 1;
            var regEx = Regex.Match(fullFileName, @"(.+) \((\d+)\)\.\w+");

            if (regEx.Success)
            {
                filename = regEx.Groups[1].Value;
                number = int.Parse(regEx.Groups[2].Value);
            }

            do
            {
                number++;

                fullFileName = Path.Combine(folder, $"{filename} ({number}){extension}");
            }
            while (System.IO.File.Exists(fullFileName));

            return fullFileName;
        }

        #endregion
    }
}
