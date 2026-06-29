using System;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Net.Web.Api.Sdk.Interfaces.File;

namespace Net.Web.Api.Sdk.Implementations.File
{
    /// <inheritdoc />
    public class FileService : IFileService
    {
        private const string UPLOAD_DIRECTORY_NAME = "Upload";

        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Uri UploadFile(byte[] content, string fileName)
        {
            var destDir = Path.Combine(_env.ContentRootPath, UPLOAD_DIRECTORY_NAME);
            if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

            var destFile = GetUniqueFileName(Path.Combine(destDir, fileName));
            System.IO.File.WriteAllBytes(destFile, content);

            var ctx = _httpContextAccessor.HttpContext;
            var request = ctx?.Request;
            var baseUrl = request != null
                ? $"{request.Scheme}://{request.Host}"
                : string.Empty;

            return new Uri($"{baseUrl}/{UPLOAD_DIRECTORY_NAME}/{Path.GetFileName(destFile)}");
        }

        private static string GetUniqueFileName(string fullFileName)
        {
            if (!System.IO.File.Exists(fullFileName)) return fullFileName;

            var folder = Path.GetDirectoryName(fullFileName);
            var name = Path.GetFileNameWithoutExtension(fullFileName);
            var ext = Path.GetExtension(fullFileName);
            var number = 1;
            var match = Regex.Match(fullFileName, @"(.+) \((\d+)\)\.\w+");
            if (match.Success)
            {
                name = match.Groups[1].Value;
                number = int.Parse(match.Groups[2].Value);
            }

            do
            {
                number++;
                fullFileName = Path.Combine(folder, $"{name} ({number}){ext}");
            }
            while (System.IO.File.Exists(fullFileName));

            return fullFileName;
        }
    }
}
