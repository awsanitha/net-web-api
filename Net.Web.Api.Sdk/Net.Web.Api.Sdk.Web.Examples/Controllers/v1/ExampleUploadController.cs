using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.File;
using Net.Web.Api.Sdk.Security.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;
using Net.Web.Api.Sdk.Web.Examples.Controllers.Common;
using Net.Web.Api.Sdk.Web.Examples.Models;
using System;
using System.IO;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.v1
{
    /// <summary>
    /// Class ExampleUploadController.
    /// </summary>
    [EnableCors]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    [ApiController]
    public sealed class ExampleUploadController : ExampleController
    {
        #region Services

        private readonly IFileService _fileService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleUploadController"/> class.
        /// </summary>
        public ExampleUploadController(IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        #endregion

        #region Public Services

        /// <summary>
        /// Uploads a file.
        /// </summary>
        [HttpPost]
        [Route(ROUTE_PREFIX + "uploadFile")]
        [TokenAuthorize]
        [SwaggerUploadOperation(typeof(UploadRequest))]
        [SwaggerMethodOrder(1)]
        [Consumes("multipart/form-data")]
        public IActionResult UploadFile([FromForm] UploadRequest parameters)
        {
            try
            {
                if (parameters?.FileInformation == null)
                {
                    return BadRequest("File is required.");
                }

                byte[] content;

                using (var ms = new MemoryStream())
                {
                    parameters.FileInformation.CopyTo(ms);
                    content = ms.ToArray();
                }

                return Ok(_fileService.UploadFile(content, parameters.FileInformation.FileName));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion
    }
}
