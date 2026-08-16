using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Extensions;
using Net.Web.Api.Sdk.Interfaces.Token;
using Net.Web.Api.Sdk.Security.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;
using Net.Web.Api.Sdk.Web.Examples.Controllers.Common;
using Net.Web.Api.Sdk.Web.Examples.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.v1
{
    /// <summary>
    /// Example token management endpoints.
    /// </summary>
    [EnableCors]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route(RouteConstants.ROUTE_PREFIX_VERSION)]
    public class ExampleTokenController : ExampleController
    {
        #region Services

        private readonly IJwtTokenService _tokenService;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ExampleTokenController"/>.
        /// </summary>
        public ExampleTokenController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        #endregion

        #region Public Endpoints

        /// <summary>
        /// Creates a new JWT Token.
        /// </summary>
        [HttpPost(ROUTE_PREFIX + "createToken")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerConsumes(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(CreateTokenResult))]
        [SwaggerResponse((int)HttpStatusCode.BadRequest, description: ResponseDescriptionConstants.INVALID_PARAMETER)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult CreateToken([FromBody] CreateTokenRequest parameters)
        {
            try
            {
                return Ok(
                    new CreateTokenResult(
                        _tokenService.CreateToken(
                            parameters.Name,
                            parameters.UniqueId,
                            parameters.Payload?.ToDictionary(c => c.Key, c => c.Value))));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Revokes a token passed in the Authorization header.
        /// </summary>
        [HttpPost(ROUTE_PREFIX + "revokeToken")]
        [TokenAuthorize]
        [SwaggerMethodOrder(2)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(bool))]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, description: ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, description: ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult RevokeToken()
        {
            try
            {
                var token = HttpContext.Request.GetToken();
                var claims = this.GetClaims()?.ToList();
                return Ok(_tokenService.RevokeToken(token, claims));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Validates a token passed in the Authorization header.
        /// </summary>
        [HttpGet(ROUTE_PREFIX + "validateToken")]
        [TokenAuthorize]
        [SwaggerMethodOrder(3)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerResponse((int)HttpStatusCode.OK, type: typeof(bool))]
        [SwaggerResponse((int)HttpStatusCode.Forbidden, description: ResponseDescriptionConstants.ACCESS_FORBIDDEN)]
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, description: ResponseDescriptionConstants.AUTHORIZATION_FAILED)]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, description: ResponseDescriptionConstants.TECHNICAL_ERROR)]
        public IActionResult ValidateToken()
        {
            try
            {
                return Ok(true);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion
    }
}
