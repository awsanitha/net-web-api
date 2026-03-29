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
using Microsoft.AspNetCore.Mvc;

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

        internal const string AUTHORIZATION_BASIC = "Basic";
        internal const string REALM = "realm";

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether to enable challenge.
        /// </summary>
        public bool EnableChallenge { get; set; }

        /// <summary>
        /// Gets or sets the realm.
        /// </summary>
        public string Realm { get; set; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Authenticates the user asynchronously.
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
                SetResult(context, HttpStatusCode.Forbidden);
                return;
            }

            if (!authHeader.StartsWith(AUTHORIZATION_BASIC + " ", StringComparison.OrdinalIgnoreCase))
            {
                SetResult(context, HttpStatusCode.Forbidden);
                return;
            }

            var parameter = authHeader.Substring(AUTHORIZATION_BASIC.Length + 1).Trim();

            if (string.IsNullOrEmpty(parameter))
            {
                SetResult(context, HttpStatusCode.Unauthorized);
                return;
            }

            var userNameAndPassword = ExtractUserNameAndPassword(parameter);

            if (userNameAndPassword == null)
            {
                SetResult(context, HttpStatusCode.Unauthorized);
                return;
            }

            var authenticationResult = new List<string>();
            var principal = await AuthenticateAsync(userNameAndPassword.Item1, userNameAndPassword.Item2, CancellationToken.None, authenticationResult);

            if (principal == null)
            {
                if (EnableChallenge)
                {
                    var challengeParam = string.IsNullOrEmpty(Realm) ? null : $@"{REALM}=""{Realm}""";
                    context.HttpContext.Response.ChallengeWith(AUTHORIZATION_BASIC, challengeParam);
                }

                context.Result = new ObjectResult(authenticationResult) { StatusCode = (int)HttpStatusCode.Unauthorized };
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

        private static void SetResult(AuthorizationFilterContext context, HttpStatusCode statusCode)
        {
            context.Result = new ObjectResult(null) { StatusCode = (int)statusCode };
        }

        #endregion
    }
}
