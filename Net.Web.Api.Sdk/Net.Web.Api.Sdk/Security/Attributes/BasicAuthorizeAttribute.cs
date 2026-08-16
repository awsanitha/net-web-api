using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Abstract base authorization filter that implements HTTP Basic authentication.
    /// Derive from this class and implement <see cref="AuthenticateAsync"/> to validate credentials.
    /// </summary>
    public abstract class BasicAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        #region Internal Constants

        internal const string AUTHORIZATION_BASIC = "Basic";
        internal const string REALM = "realm";

        #endregion

        #region Public Properties

        /// <summary>Gets or sets whether to add a WWW-Authenticate challenge header on 401.</summary>
        public bool EnableChallenge { get; set; }

        /// <summary>Gets or sets the realm value in the WWW-Authenticate header.</summary>
        public string Realm { get; set; }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Validates the supplied credentials and returns the resulting principal, or <c>null</c> on failure.
        /// </summary>
        protected abstract Task<IPrincipal> AuthenticateAsync(
            string userName,
            string password,
            CancellationToken cancellationToken,
            IList<string> authenticationResult);

        #endregion

        #region IAsyncAuthorizationFilter

        /// <inheritdoc />
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers[HeaderNames.Authorization].ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                context.Result = StatusCodeResult(HttpStatusCode.Forbidden);
                return;
            }

            if (!authHeader.StartsWith(AUTHORIZATION_BASIC + " ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = StatusCodeResult(HttpStatusCode.Forbidden);
                return;
            }

            var parameter = authHeader.Substring(AUTHORIZATION_BASIC.Length).Trim();

            if (string.IsNullOrEmpty(parameter))
            {
                context.Result = StatusCodeResult(HttpStatusCode.Unauthorized);
                ApplyChallenge(context);
                return;
            }

            var credentials = ExtractCredentials(parameter);
            if (credentials == null)
            {
                context.Result = StatusCodeResult(HttpStatusCode.Unauthorized);
                ApplyChallenge(context);
                return;
            }

            var authResult = new List<string>();
            var principal = await AuthenticateAsync(
                credentials.Item1,
                credentials.Item2,
                context.HttpContext.RequestAborted,
                authResult);

            if (principal == null)
            {
                context.Result = new ObjectResult(authResult) { StatusCode = (int)HttpStatusCode.Unauthorized };
                ApplyChallenge(context);
            }
            else
            {
                context.HttpContext.User = (System.Security.Claims.ClaimsPrincipal)principal;
            }
        }

        #endregion

        #region Private Helpers

        private static IActionResult StatusCodeResult(HttpStatusCode code)
            => new StatusCodeResult((int)code);

        private void ApplyChallenge(AuthorizationFilterContext context)
        {
            if (!EnableChallenge) return;

            var parameter = string.IsNullOrEmpty(Realm)
                ? null
                : $@"{REALM}=""{Realm}""";

            var value = parameter == null
                ? AUTHORIZATION_BASIC
                : $"{AUTHORIZATION_BASIC} {parameter}";

            context.HttpContext.Response.Headers[HeaderNames.WWWAuthenticate] = value;
        }

        private static Tuple<string, string> ExtractCredentials(string parameter)
        {
            byte[] credentialBytes;
            try
            {
                credentialBytes = Convert.FromBase64String(parameter);
            }
            catch (FormatException)
            {
                return null;
            }

            var encoding = (Encoding)Encoding.ASCII.Clone();
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;

            string decoded;
            try
            {
                decoded = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return null;
            }

            if (string.IsNullOrEmpty(decoded)) return null;

            var colon = decoded.IndexOf(':');
            if (colon == -1) return null;

            return Tuple.Create(decoded.Substring(0, colon), decoded.Substring(colon + 1));
        }

        #endregion
    }
}
