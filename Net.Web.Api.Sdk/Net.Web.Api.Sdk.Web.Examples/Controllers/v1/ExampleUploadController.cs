using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.File;
using Net.Web.Api.Sdk.Security.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;
using Net.Web.Api.Sdk.Web.Examples.Controllers.Common;
using Net.Web.Api.Sdk.Web.Examples.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.v1
{
    /// <summary>
    /// Example file upload endpoint.
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

        /// <summary>
        /// Initializes a new instance of <see cref="ExampleUploadController"/>.
        /// </summary>
        public ExampleUploadController(IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Uploads a file.
        /// </summary>
        [HttpPost(ROUTE_PREFIX + "uploadFile")]
        [TokenAuthorize]
        [SwaggerUploadOperation(typeof(UploadRequest))]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.OTHER })]
        [SwaggerConsumes(ConsumerProducerConstants.MULTIPART)]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, description: ResponseDescriptionConstants.INVALID_PARAMETER)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, description: ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, description: ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public async Task<IActionResult> UploadFile([FromForm] UploadRequest parameters)
        {
            try
            {
                using var ms = new System.IO.MemoryStream();
                await parameters.FileInformation.CopyToAsync(ms);

                return Ok(_fileService.UploadFile(
                    ms.ToArray(),
                    parameters.FileInformation.FileName));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
