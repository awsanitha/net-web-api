using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Net.Web.Api.Sdk.Extensions
{
    /// <summary>
    /// Class ControllerExtensions.
    /// </summary>
    public static class ControllerExtensions
    {
        #region Public Extensions

        /// <summary>
        /// Gets the claims.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns>IList&lt;Claim&gt;.</returns>
        public static IList<Claim> GetClaims(this ControllerBase controller)
        {
            var identity = controller.HttpContext.User.Identity;

            if (identity == null || !identity.IsAuthenticated)
            {
                return null;
            }

            return ((ClaimsIdentity)identity).Claims.ToList();
        }

        #endregion
    }
}
