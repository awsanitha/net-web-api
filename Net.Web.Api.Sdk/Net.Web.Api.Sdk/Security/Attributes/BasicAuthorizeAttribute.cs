using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class BasicAuthorizeAttribute.
    /// Implements <see cref="Attribute" /> and <see cref="IAsyncAuthorizationFilter" />
    /// </summary>
    public abstract class BasicAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        #region Internal Constants

        /// <summary>
        /// The authorization basic
        /// </summary>
        internal const string AUTHORIZATION_BASIC = "Basic";

        /// <summary>
        /// The realm
        /// </summary>
        internal const string REALM = "realm";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether [enable challenge].
        /// </summary>
        /// <value><c>true</c> if [enable challenge]; otherwise, <c>false</c>.</value>
        public bool EnableChallenge { get; set; }

        /// <summary>
        /// Gets or sets the realm.
        /// </summary>
        /// <value>The realm.</value>
        public string Realm { get; set; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Authenticates the asynchronous.
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="authenticationResult">The authentication result.</param>
        /// <returns>Task&lt;IPrincipal&gt;.</returns>
        protected abstract Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken, IList<string> authenticationResult);

        #endregion

        #region IAsyncAuthorizationFilter Implementations

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            if (!authHeader.StartsWith(AUTHORIZATION_BASIC + " ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            var parameter = authHeader.Substring(AUTHORIZATION_BASIC.Length + 1).Trim();

            if (string.IsNullOrEmpty(parameter))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                SetChallenge(context);
                return;
            }

            var userNameAndPassword = ExtractUserNameAndPassword(parameter);

            if (userNameAndPassword == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                SetChallenge(context);
                return;
            }

            var authenticationResult = new List<string>();
            var principal = await AuthenticateAsync(
                userNameAndPassword.Item1,
                userNameAndPassword.Item2,
                context.HttpContext.RequestAborted,
                authenticationResult);

            if (principal == null)
            {
                context.Result = new ObjectResult(authenticationResult)
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                SetChallenge(context);
            }
            else
            {
                context.HttpContext.User = (System.Security.Claims.ClaimsPrincipal)principal;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts the user name and password from the Basic auth parameter.
        /// </summary>
        private static Tuple<string, string> ExtractUserNameAndPassword(string authorizationParameter)
        {
            byte[] credentialBytes;

            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return null;
            }

            var encoding = Encoding.ASCII;
            encoding = (Encoding)encoding.Clone();
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;

            string decodedCredentials;

            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return null;
            }

            if (string.IsNullOrEmpty(decodedCredentials))
            {
                return null;
            }

            var colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
            {
                return null;
            }

            var userName = decodedCredentials.Substring(0, colonIndex);
            var password = decodedCredentials.Substring(colonIndex + 1);

            return new Tuple<string, string>(userName, password);
        }

        /// <summary>
        /// Sets the WWW-Authenticate challenge header on the response.
        /// </summary>
        private void SetChallenge(AuthorizationFilterContext context)
        {
            if (!EnableChallenge)
            {
                return;
            }

            var parameter = string.IsNullOrEmpty(Realm)
                ? null
                : $@"{REALM}=""{Realm}""";

            var challengeValue = parameter == null
                ? AUTHORIZATION_BASIC
                : $"{AUTHORIZATION_BASIC} {parameter}";

            context.HttpContext.Response.Headers.WWWAuthenticate = challengeValue;
        }

        #endregion
    }
}
