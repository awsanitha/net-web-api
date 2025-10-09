using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Class BasicAuthorizeAttribute.
    /// Implements the <see cref="Attribute" />
    /// Implements the <see cref="IAuthorizationFilter" />
    /// </summary>
    /// <seealso cref="Attribute" />
    /// <seealso cref="IAuthorizationFilter" />
    public abstract class BasicAuthorizeAttribute : Attribute, IAuthorizationFilter
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

        #region IAuthorizationFilter Implementation

        /// <summary>
        /// Called early in the filter pipeline to confirm request is authorized.
        /// </summary>
        /// <param name="context">The authorization filter context.</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers["Authorization"].ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith(AUTHORIZATION_BASIC))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var credentials = ExtractCredentials(authHeader);
            if (credentials == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var principal = AuthenticateUser(credentials.Username, credentials.Password);
            if (principal == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            context.HttpContext.User = principal as ClaimsPrincipal ?? new ClaimsPrincipal(principal);
        }

        #endregion

        #region Protected Abstract Methods

        /// <summary>
        /// Authenticates the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>IPrincipal.</returns>
        protected abstract IPrincipal AuthenticateUser(string username, string password);

        #endregion

        #region Private Methods

        /// <summary>
        /// Extracts the credentials from the authorization header.
        /// </summary>
        /// <param name="authHeader">The authorization header.</param>
        /// <returns>BasicCredentials.</returns>
        private static BasicCredentials ExtractCredentials(string authHeader)
        {
            try
            {
                var encodedCredentials = authHeader.Substring(AUTHORIZATION_BASIC.Length).Trim();
                var credentialBytes = Convert.FromBase64String(encodedCredentials);
                var credentials = Encoding.UTF8.GetString(credentialBytes);
                var parts = credentials.Split(':');

                if (parts.Length == 2)
                {
                    return new BasicCredentials { Username = parts[0], Password = parts[1] };
                }
            }
            catch
            {
                // Invalid credentials format
            }

            return null;
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Class BasicCredentials.
        /// </summary>
        private class BasicCredentials
        {
            /// <summary>
            /// Gets or sets the username.
            /// </summary>
            /// <value>The username.</value>
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets the password.
            /// </summary>
            /// <value>The password.</value>
            public string Password { get; set; }
        }

        #endregion
    }
}
