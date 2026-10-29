using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <summary>
    /// Class SwaggerFilterHelper.
    /// Replacement of the Web API <c>ApiDescription.GetControllerAndActionAttributes</c> helper,
    /// which has no ASP.NET Core equivalent.
    /// </summary>
    internal static class SwaggerFilterHelper
    {
        #region Internal Methods

        /// <summary>
        /// Gets the attributes declared on the action method and on its controller type.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="context">The operation filter context.</param>
        /// <returns>IEnumerable&lt;T&gt;.</returns>
        internal static IEnumerable<T> GetControllerAndActionAttributes<T>(OperationFilterContext context) where T : Attribute
        {
            var result = new List<T>();

            if (context == null)
            {
                return result;
            }

            var methodInfo = context.MethodInfo
                ?? (context.ApiDescription?.ActionDescriptor as ControllerActionDescriptor)?.MethodInfo;

            if (methodInfo != null)
            {
                result.AddRange(methodInfo.GetCustomAttributes(typeof(T), true).Cast<T>());

                if (methodInfo.DeclaringType != null)
                {
                    result.AddRange(methodInfo.DeclaringType.GetCustomAttributes(typeof(T), true).Cast<T>());
                }
            }

            return result;
        }

        #endregion
    }
}
