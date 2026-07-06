using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
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
    public class FileService : IFileService
    {
        #region Constants

        private const string UPLOAD_DIRECTORY_NAME = "Upload";

        #endregion

        #region Private Fields

        private readonly string _rootPath;
        private readonly IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        public FileService(IHostEnvironment hostEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _rootPath = hostEnvironment?.ContentRootPath ?? AppDomain.CurrentDomain.BaseDirectory;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region IFileService Implementations

        /// <summary>
        /// Uploads the file.
        /// </summary>
        public Uri UploadFile(byte[] content, string fileName)
        {
            var destinationDirectory = Path.Combine(_rootPath, UPLOAD_DIRECTORY_NAME);

            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            var destinationFile = GetUniqueFileName(Path.Combine(destinationDirectory, fileName));

            System.IO.File.WriteAllBytes(destinationFile, content);

            var request = _httpContextAccessor?.HttpContext?.Request;

            if (request != null)
            {
                var baseUrl = $"{request.Scheme}://{request.Host}";
                return new Uri($"{baseUrl}/{UPLOAD_DIRECTORY_NAME}/{Path.GetFileName(destinationFile)}");
            }

            return new Uri($"file:///{destinationFile.Replace('\\', '/')}");
        }

        #endregion

        #region Private Methods

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
