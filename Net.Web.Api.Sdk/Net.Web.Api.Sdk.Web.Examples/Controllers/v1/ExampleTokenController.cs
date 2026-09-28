using Asp.Versioning;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;
using Net.Web.Api.Sdk.Web.Examples.Controllers.Common;
using Net.Web.Api.Sdk.Web.Examples.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.v1
{
    /// <summary>
    /// Class ExampleTokenController.
    /// Implements the <see cref="ExampleController" />
    /// </summary>
    /// <seealso cref="ExampleController" />
    [EnableCors]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    public class ExampleTokenController : ExampleController
    {       
        #region Services

        /// <summary>
        /// The token service
        /// </summary>
        private readonly IJwtTokenService _tokenService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleTokenController"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        /// <exception cref="ArgumentNullException">tokenService</exception>
        public ExampleTokenController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        #endregion

        #region Public Services

        /// <summary>
        /// Creates a new JWT Token.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [Route(ROUTE_PREFIX + "createToken")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerConsumes(ConsumerProducerConstants.JSON)]
        [SwaggerResponse(200, Type = typeof(CreateTokenResult))]
        [SwaggerResponse(400, Type = typeof(IList<string>), Description = ResponseDescriptionConstants.INVALID_PARAMETER)]
        [SwaggerResponse(500, Description = ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult CreateToken(CreateTokenRequest paramaters)
        {
            try
            {
                return Ok(
                    new CreateTokenResult(
                        _tokenService.CreateToken(
                            paramaters.Name,
                            paramaters.UniqueId,
                            paramaters.Payload != null ? paramaters.Payload.ToDictionary(c => c.Key, c => c.Value) : null
                        )
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Revokes a token passed in the authorization header.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [Route(ROUTE_PREFIX + "revokeToken")]
        [TokenAuthorize]
        [SwaggerMethodOrder(2)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse(200, Type = typeof(bool))]
        [SwaggerResponse(403, Description = ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse(401, Description = ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse(500, Description = ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult RevokeToken()
        {
            try
            {
                var token = HttpContext.GetToken();
                var claims = this.GetClaims().ToList();

                return Ok(_tokenService.RevokeToken(token, claims));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Validates a token passed in the authorization header.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet]
        [Route(ROUTE_PREFIX + "validateToken")]
        [TokenAuthorize]
        [SwaggerMethodOrder(3)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse(200, Type = typeof(bool))]
        [SwaggerResponse(403, Description = ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse(401, Description = ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse(500, Description = ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult ValidateToken()
        {
            try
            {
                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion
    }
}
