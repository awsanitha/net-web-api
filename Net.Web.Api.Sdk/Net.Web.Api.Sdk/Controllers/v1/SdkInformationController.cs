using Asp.Versioning;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.Information;
using Newtonsoft.Json.Linq;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Controllers.v1
{
    /// <summary>
    /// Class SdkInformationController.
    /// Implements the <see cref="SdkController" />
    /// </summary>
    /// <seealso cref="SdkController" />
    [EnableCors]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    [ApiController]
    public class SdkInformationController : SdkController
    {
        #region Services

        /// <summary>
        /// The information service
        /// </summary>
        private readonly IInformationService _informationService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SdkInformationController"/> class.
        /// </summary>
        /// <param name="informationService">The information service.</param>
        public SdkInformationController(IInformationService informationService)
        {
            _informationService = informationService ?? throw new ArgumentNullException(nameof(informationService));
        }

        #endregion

        #region Public Services

        /// <summary>
        /// Returns the .Net Web API SDK informations.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [Route("sdk/informations")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        public IActionResult GetSdkInformations()
        {
            try
            {
                return Ok(_informationService.GetSdkInformations());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion
    }
}
