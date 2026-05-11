using Net.Web.Api.Sdk.Common.Http;
using Net.Web.Api.Sdk.Extensions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class BasicAuthorizeAttribute.
    /// Implements the <see cref="Attribute" />
    /// Implements the <see cref="IAsyncAuthorizationFilter" />
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
        public bool EnableChallenge { get; set; }

        /// <summary>
        /// Gets or sets the realm.
        /// </summary>
        public string Realm { get; set; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Authenticates the asynchronous.
        /// </summary>
        protected abstract Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken, IList<string> authenticationResult);

        #endregion

        #region IAsyncAuthorizationFilter Implementations

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Forbidden);
                return;
            }

            if (!authHeader.StartsWith(AUTHORIZATION_BASIC + " "))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Forbidden);
                return;
            }

            var parameter = authHeader.Substring(AUTHORIZATION_BASIC.Length + 1).Trim();

            if (string.IsNullOrEmpty(parameter))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized);
                return;
            }

            var userNameAndPassword = ExtractUserNameAndPassword(parameter);

            if (userNameAndPassword == null)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized);
                return;
            }

            var authenticationResult = new List<string>();
            var userName = userNameAndPassword.Item1;
            var password = userNameAndPassword.Item2;
            var principal = await AuthenticateAsync(userName, password, context.HttpContext.RequestAborted, authenticationResult);

            if (principal == null)
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, authenticationResult);

                if (EnableChallenge)
                {
                    var parameter2 = string.IsNullOrEmpty(Realm) ? null : $@"{REALM}=""{Realm}""";
                    context.ChallengeWith(AUTHORIZATION_BASIC, parameter2);
                }
            }
            else
            {
                context.HttpContext.User = (System.Security.Claims.ClaimsPrincipal)principal;
            }
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
