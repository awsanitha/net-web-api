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
    /// Class BasicAuthorizeAttribute. Provides HTTP Basic authentication for ASP.NET Core actions.
    /// </summary>
    public abstract class BasicAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        #region Internal Constants

        internal const string AUTHORIZATION_BASIC = "Basic";
        internal const string REALM = "realm";

        #endregion

        #region Public Properties

        public bool EnableChallenge { get; set; }
        public string Realm { get; set; }

        #endregion

        #region Abstract Methods

        protected abstract Task<IPrincipal> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken, IList<string> authenticationResult);

        #endregion

        #region IAsyncAuthorizationFilter Implementation

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                AddChallenge(context);
                return;
            }

            if (!authHeader.StartsWith(AUTHORIZATION_BASIC + " ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                AddChallenge(context);
                return;
            }

            var parameter = authHeader.Substring(AUTHORIZATION_BASIC.Length + 1).Trim();

            if (string.IsNullOrEmpty(parameter))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                AddChallenge(context);
                return;
            }

            var userNameAndPassword = ExtractUserNameAndPassword(parameter);

            if (userNameAndPassword == null)
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                AddChallenge(context);
                return;
            }

            var authenticationResult = new List<string>();
            var principal = await AuthenticateAsync(userNameAndPassword.Item1, userNameAndPassword.Item2, context.HttpContext.RequestAborted, authenticationResult);

            if (principal == null)
            {
                context.Result = new ObjectResult(authenticationResult) { StatusCode = (int)HttpStatusCode.Unauthorized };
                AddChallenge(context);
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
                return null;

            var colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
                return null;

            var userName = decodedCredentials.Substring(0, colonIndex);
            var password = decodedCredentials.Substring(colonIndex + 1);

            return new Tuple<string, string>(userName, password);
        }

        private void AddChallenge(AuthorizationFilterContext context)
        {
            if (!EnableChallenge)
                return;

            var parameter = string.IsNullOrEmpty(Realm) ? null : $@"{REALM}=""{Realm}""";
            var challengeValue = string.IsNullOrEmpty(parameter) ? AUTHORIZATION_BASIC : $"{AUTHORIZATION_BASIC} {parameter}";
            context.HttpContext.Response.Headers["WWW-Authenticate"] = challengeValue;
        }

        #endregion
    }
}
