using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Net.Web.Api.Sdk.Security.Attributes
{
    /// <summary>
    /// Abstract base for Basic authentication authorization filters.
    /// </summary>
    public abstract class BasicAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
    {
        internal const string AUTHORIZATION_BASIC = "Basic";
        internal const string REALM = "realm";

        public bool EnableChallenge { get; set; }
        public string Realm { get; set; }

        protected abstract Task<IPrincipal> AuthenticateAsync(string userName, string password,
            CancellationToken cancellationToken, IList<string> authenticationResult);

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;
            var authHeader = request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }

            if (!authHeader.StartsWith(AUTHORIZATION_BASIC + " ", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }

            var parameter = authHeader.Substring(AUTHORIZATION_BASIC.Length + 1).Trim();

            if (string.IsNullOrEmpty(parameter))
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                return;
            }

            var credentials = ExtractUserNameAndPassword(parameter);
            if (credentials == null)
            {
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
                return;
            }

            var authResult = new List<string>();
            var principal = await AuthenticateAsync(credentials.Item1, credentials.Item2,
                context.HttpContext.RequestAborted, authResult);

            if (principal == null)
            {
                if (EnableChallenge && !string.IsNullOrEmpty(Realm))
                    context.HttpContext.Response.Headers.WWWAuthenticate =
                        $"{AUTHORIZATION_BASIC} {REALM}=\"{Realm}\"";
                context.Result = new StatusCodeResult((int)HttpStatusCode.Unauthorized);
            }
            else
            {
                context.HttpContext.User = (System.Security.Claims.ClaimsPrincipal)principal;
            }
        }

        private static Tuple<string, string> ExtractUserNameAndPassword(string parameter)
        {
            byte[] credentialBytes;
            try { credentialBytes = Convert.FromBase64String(parameter); }
            catch (FormatException) { return null; }

            var encoding = Encoding.ASCII;
            encoding = (Encoding)encoding.Clone();
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;

            string decoded;
            try { decoded = encoding.GetString(credentialBytes); }
            catch (DecoderFallbackException) { return null; }

            if (string.IsNullOrEmpty(decoded)) return null;

            var colon = decoded.IndexOf(':');
            if (colon == -1) return null;

            return new Tuple<string, string>(decoded.Substring(0, colon), decoded.Substring(colon + 1));
        }
    }
}
