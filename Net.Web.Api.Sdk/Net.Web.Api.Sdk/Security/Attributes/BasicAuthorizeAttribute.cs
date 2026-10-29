using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Net.Web.Api.Sdk.Common.Http;
using Net.Web.Api.Sdk.Extensions;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class BasicAuthorizeAttribute.
    /// Implements the <see cref="Attribute" />
    /// Implements the <see cref="IAsyncAuthorizationFilter" />
    /// </summary>
    /// <seealso cref="Attribute" />
    /// <seealso cref="IAsyncAuthorizationFilter" />
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

        #region Abscrtact Methods

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

        #region Attribute Overrides

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets a value indicating whether more than one instance of the indicated attribute can be specified for a single program element.
        /// </summary>
        /// <value><c>true</c> if [allow multiple]; otherwise, <c>false</c>.</value>
        public virtual bool AllowMultiple => false;

        #endregion

        #region IAsyncAuthorizationFilter Implementations

        /// <summary>
        /// authorize as an asynchronous operation.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        /// <returns>A Task that will perform authentication.</returns>
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var cancellationToken = context.HttpContext.RequestAborted;
            var authorization = context.HttpContext.Request.Headers[HeaderNames.Authorization].ToString();

            if (string.IsNullOrEmpty(authorization))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Forbidden);

                return;
            }

            var separatorIndex = authorization.IndexOf(' ');
            var scheme = separatorIndex > 0 ? authorization.Substring(0, separatorIndex) : authorization;

            if (!AUTHORIZATION_BASIC.Equals(scheme, StringComparison.Ordinal))
            {
                context.Result = new ResponseActionResult(HttpStatusCode.Forbidden);

                return;
            }

            var parameter = separatorIndex > 0 ? authorization.Substring(separatorIndex + 1).Trim() : string.Empty;

            if (string.IsNullOrEmpty(parameter))
            {
                Unauthorized(context, null);

                return;
            }

            var userNameAndPasword = ExtractUserNameAndPassword(parameter);

            if (userNameAndPasword == null)
            {
                Unauthorized(context, null);

                return;
            }

            var authenticationResult = new List<string>();
            var userName = userNameAndPasword.Item1;
            var password = userNameAndPasword.Item2;
            var principal = await AuthenticateAsync(userName, password, cancellationToken, authenticationResult);

            if (principal == null)
            {
                Unauthorized(context, authenticationResult);
            }
            else
            {
                context.HttpContext.User = principal as ClaimsPrincipal ?? new ClaimsPrincipal(principal.Identity);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Sets an unauthorized result and appends the challenge when challenging is enabled.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="content">The content.</param>
        private void Unauthorized(AuthorizationFilterContext context, object content)
        {
            context.Result = new ResponseActionResult(HttpStatusCode.Unauthorized, content);

            if (EnableChallenge)
            {
                Challenge(context);
            }
        }

        /// <summary>
        /// Extracts the user name and password.
        /// </summary>
        /// <param name="authorizationParameter">The authorization parameter.</param>
        /// <returns>TupleModel&lt;System.String, System.String&gt;.</returns>
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
        /// Challenges the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        private void Challenge(AuthorizationFilterContext context)
        {
            var parameter = string.IsNullOrEmpty(Realm) ? null : $"{REALM}=\"{Realm}\"";

            if (string.IsNullOrEmpty(parameter))
            {
                context.ChallengeWith(AUTHORIZATION_BASIC);

                return;
            }

            context.ChallengeWith(AUTHORIZATION_BASIC, parameter);
        }

        #endregion
    }
}
