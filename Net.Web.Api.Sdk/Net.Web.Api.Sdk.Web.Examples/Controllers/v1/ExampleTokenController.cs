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
    /// Class ExampleTokenController. Example controller demonstrating JWT token operations.
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
        /// Initializes a new instance of the <see cref="ExampleTokenController"/> class.
        /// </summary>
        /// <param name="tokenService">The token service.</param>
        public ExampleTokenController(IJwtTokenService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        #endregion

        #region Public Services

        /// <summary>
        /// Creates a new JWT Token.
        /// </summary>
        [HttpPost]
        [Route(ROUTE_PREFIX + "createToken")]
        [AllowAnonymous]
        [SwaggerMethodOrder(1)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [SwaggerConsumes(ConsumerProducerConstants.JSON)]
        [ProducesResponseType(typeof(CreateTokenResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IList<string>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult CreateToken([FromBody] CreateTokenRequest paramaters)
        {
            try
            {
                return Ok(
                    new CreateTokenResult(
                        _tokenService.CreateToken(
                            paramaters.Name!,
                            paramaters.UniqueId!,
                            paramaters.Payload != null ? paramaters.Payload.ToDictionary(c => c.Key, c => c.Value) : null
                        )
                    )
                );
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Revokes a token passed in the authorization header.
        /// </summary>
        [HttpPost]
        [Route(ROUTE_PREFIX + "revokeToken")]
        [TokenAuthorize]
        [SwaggerMethodOrder(2)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public IActionResult RevokeToken()
        {
            try
            {
                var token = HttpContext.Request.GetToken();
                var claims = this.GetClaims()?.ToList();

                if (string.IsNullOrEmpty(token) || claims == null)
                {
                    return Unauthorized();
                }

                return Ok(_tokenService.RevokeToken(token, claims));
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Validates a token passed in the authorization header.
        /// </summary>
        [HttpGet]
        [Route(ROUTE_PREFIX + "validateToken")]
        [TokenAuthorize]
        [SwaggerMethodOrder(3)]
        [SwaggerOperation(Tags = new[] { ExampleControllerGroups.SECURITY })]
        [SwaggerProduces(ConsumerProducerConstants.JSON)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
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
