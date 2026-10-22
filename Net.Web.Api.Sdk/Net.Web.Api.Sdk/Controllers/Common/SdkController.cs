using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Common.Constants;

namespace Net.Web.Api.Sdk.Controllers.Common
{
    /// <summary>
    /// Class SdkController.
    /// Implements the <see cref="ControllerBase" />
    /// </summary>
    [ApiController]
    [SwaggerOperationOrder(From = SwaggerOperationOrderAttribute.OperationFrom.Sdk,
        OperationTags = new[] {
            SwaggerSdkConstants.ABOUT
        })]
    public class SdkController : ControllerBase {}
}
