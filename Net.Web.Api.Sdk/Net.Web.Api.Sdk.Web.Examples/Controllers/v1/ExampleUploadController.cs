using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
    /// Class ExampleUploadController. This class cannot be inherited.
    /// Implements the <see cref="ExampleController" />
    /// </summary>
    /// <seealso cref="ExampleController" />
    [EnableCors(SdkCorsConstants.DEFAULT_POLICY_NAME)]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    public sealed class ExampleUploadController : ExampleController
    {
        #region Services

        /// <summary>
        /// The file service
        /// </summary>
        private readonly IFileService _fileService;

        #endregion

        #region Constructors


        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleUploadController"/> class.
        /// </summary>
        /// <param name="fileService">The file service.</param>
        /// <exception cref="ArgumentNullException">fileService</exception>
        public ExampleUploadController(IFileService fileService)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        #endregion

        #region Public Services

        /// <summary>
        /// Uploads a file.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [Route(ROUTE_PREFIX + "uploadFile")]
        [TokenAuthorize]
        [SwaggerUploadOperation(typeof(UploadRequest))]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.OTHER })]
        [SwaggerConsumes(ConsumerProducerConstants.MULTIPART)]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(IList<string>), Description = ResponseDescriptionConstants.INVALID_PARAMETER)]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, Description = ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult UploadFile([FromForm] UploadRequest parameters)
        {
            try
            {
                var file = parameters.FileInformation;

                byte[] content;

                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);

                    content = stream.ToArray();
                }

                return Ok(_fileService.UploadFile(content, file.FileName));
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion

    }
}
