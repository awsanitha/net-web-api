using Asp.Versioning;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.Information;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Net.Web.Api.Sdk.Controllers.v1
{
    /// <summary>
    /// Class SdkInformationController.
    /// Implements the <see cref="SdkController" />
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
        /// Initializes a new instance of the <see cref="SdkInformationController"/> class.
        /// </summary>
        public SdkInformationController(IInformationService informationService)
        {
            _informationService = informationService ?? throw new ArgumentNullException(nameof(informationService));
        }

        #endregion

        #region Public Services

        /// <summary>
        /// Returns the .Net Web API SDK informations.
        /// </summary>
        [HttpGet]
        [Route("sdk/informations")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { SwaggerSdkConstants.ABOUT })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(JObject))]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = ResponseDescriptionConstants.TECHNICAL_ERROR)]
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
