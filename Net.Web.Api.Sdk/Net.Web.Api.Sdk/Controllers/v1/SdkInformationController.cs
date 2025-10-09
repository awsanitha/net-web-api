using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Interfaces.Information;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using System;

namespace Net.Web.Api.Sdk.Controllers.v1
{
    /// <summary>
    /// Class SdkInformationController.
    /// Implements the <see cref="SdkController" />
    /// </summary>
    /// <seealso cref="SdkController" />
    [EnableCors("AllowAll")]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}")]
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
        /// <returns>ActionResult.</returns>
        [HttpGet("sdk/informations")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [ProducesResponseType(typeof(JObject), 200)]
        [ProducesResponseType(500)]
        public ActionResult<JObject> GetSdkInformations()
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
