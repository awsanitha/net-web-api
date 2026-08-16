using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="ControllerBase"/>.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns the claims of the currently authenticated principal, or <c>null</c> if unauthenticated.
        /// </summary>
        public static IList<Claim> GetClaims(this ControllerBase controller)
        {
            var identity = controller?.User?.Identity;

            if (identity == null || !identity.IsAuthenticated)
                return null;

            return ((ClaimsIdentity)identity).Claims.ToList();
        }
    }
}
