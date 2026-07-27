using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Net.Web.Api.Sdk.Controllers.Common
{
    /// <summary>
    /// Class SdkController.
    /// Implements the <see cref="ControllerBase" />
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [SwaggerOperationOrder(From = SwaggerOperationOrderAttribute.OperationFrom.Sdk,
        OperationTags = new[] {
            SwaggerSdkConstants.ABOUT
        })]
    [ApiController]
    public class SdkController : ControllerBase {}
}
