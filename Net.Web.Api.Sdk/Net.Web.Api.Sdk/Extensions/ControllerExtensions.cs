using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Extension methods for ASP.NET Core controllers.
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Returns the authenticated claims for the current request.
        /// </summary>
        public static IList<Claim> GetClaims(this ControllerBase controller)
        {
            var identity = controller.HttpContext.User?.Identity;
            if (identity == null || !identity.IsAuthenticated) return null;
            return ((ClaimsIdentity)identity).Claims.ToList();
        }
    }
}
