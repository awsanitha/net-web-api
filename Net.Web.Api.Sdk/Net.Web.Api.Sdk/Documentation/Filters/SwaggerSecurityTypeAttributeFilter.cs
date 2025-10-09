using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Documentation.Constants;
using Net.Web.Api.Sdk.Security.Attributes;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace Net.Web.Api.Sdk.Documentation.Filters
{
    /// <inheritdoc />
    /// <summary>
    /// Class SwaggerSecurityTypeAttributeFilter.
    /// </summary>
    public class SwaggerSecurityTypeAttributeFilter : IOperationFilter
    {
        #region IOperationFilter Implementation

        /// <inheritdoc />
        /// <summary>
        /// Applies the specified operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var securityType = GetSecurityType(context);

            if (!string.IsNullOrEmpty(securityType))
            {
                operation.Description = securityType;
            }
            else
            {
                var attr = context.MethodInfo.GetCustomAttributes(typeof(SwaggerSecurityTypeAttribute), false)
                    .Cast<SwaggerSecurityTypeAttribute>()
                    .FirstOrDefault();

                if (attr != null)
                {
                    operation.Description = attr.SecurityType;
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the type of the security.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.String.</returns>
        private static string GetSecurityType(OperationFilterContext context)
        {
            var methodInfo = context.MethodInfo;
            var controllerType = methodInfo.DeclaringType;

            if (controllerType == null)
            {
                return null;
            }

            // Check for AllowAnonymous on method
            var customAttribute = methodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).FirstOrDefault();

            if (customAttribute != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check for TokenAuthorize on method
            customAttribute = methodInfo.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault();

            if (customAttribute != null)
            {
                return SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(customAttribute);
            }

            // Check for BasicAuthorize on method
            customAttribute = methodInfo.GetCustomAttributes(typeof(BasicAuthorizeAttribute), true).FirstOrDefault();

            if (customAttribute != null)
            {
                return SwaggerSecurityTypeConstants.BASIC_SECURED;
            }

            // Check for AllowAnonymous on controller
            customAttribute = controllerType.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).FirstOrDefault();

            if (customAttribute != null)
            {
                return SwaggerSecurityTypeConstants.ANONYMOUS;
            }

            // Check for TokenAuthorize on controller
            customAttribute = controllerType.GetCustomAttributes(typeof(TokenAuthorizeAttribute), true).FirstOrDefault();

            return customAttribute != null
                ? SwaggerSecurityTypeConstants.TOKEN_SECURED + GetIntendedAudiences(customAttribute)
                : SwaggerSecurityTypeConstants.ANONYMOUS;
        }

        /// <summary>
        /// Gets the intended audiences.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetIntendedAudiences(object attribute)
        {
            if (!(attribute is TokenAuthorizeAttribute tokenAuthorizeAttribute))
            {
                return string.Empty;
            }

            var audienceString = tokenAuthorizeAttribute.IntendedAudiences;

            return string.IsNullOrEmpty(audienceString) ? string.Empty : $"<br/>Intended Audience(s): {audienceString}";
        }

        #endregion
    }
}
