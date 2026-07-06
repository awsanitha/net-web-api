using Asp.Versioning;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.File;
using Net.Web.Api.Sdk.Security.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;
using Net.Web.Api.Sdk.Web.Examples.Controllers.Common;
using Net.Web.Api.Sdk.Web.Examples.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.v1
{
    /// <summary>
    /// Class ExampleUploadController.
    /// </summary>
    [EnableCors]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    public sealed class ExampleUploadController : ExampleController
    {
        #region Services

        private readonly IFileService _fileService;

        #endregion

        #region Constructors

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
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.OTHER })]
        [SwaggerConsumes(ConsumerProducerConstants.MULTIPART)]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, type: typeof(IList<string>), description: ResponseDescriptionConstants.INVALID_PARAMETER)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, description: ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, description: ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult UploadFile([FromForm] UploadRequest parameters)
        {
            try
            {
                if (parameters?.FileInformation == null)
                {
                    return BadRequest("File is required.");
                }

                byte[] fileBytes;

                using (var memoryStream = new MemoryStream())
                {
                    parameters.FileInformation.CopyTo(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                var result = _fileService.UploadFile(fileBytes, parameters.FileInformation.FileName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
