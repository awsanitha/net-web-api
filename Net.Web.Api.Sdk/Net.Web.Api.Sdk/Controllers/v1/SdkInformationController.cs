using System;
using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.Information;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;

namespace Net.Web.Api.Sdk.Controllers.v1
{
    /// <summary>
    /// Returns SDK information (version, loaded tokens, etc.)
    /// </summary>
    [EnableCors]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    public class SdkInformationController : SdkController
    {
        #region Services

        private readonly IInformationService _informationService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="SdkInformationController"/>.
        /// </summary>
        public SdkInformationController(IInformationService informationService)
        {
            _informationService = informationService ?? throw new ArgumentNullException(nameof(informationService));
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Returns the .Net Web API SDK informations.
        /// </summary>
        [HttpGet("sdk/informations")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { SwaggerSdkConstants.ABOUT })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(JObject))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult GetSdkInformations()
        {
            try
            {
                return Ok(_informationService.GetSdkInformations());
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
