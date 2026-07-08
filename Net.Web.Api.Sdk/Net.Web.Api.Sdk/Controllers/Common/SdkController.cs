using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;

namespace Net.Web.Api.Sdk.Controllers.Common
{
    /// <summary>
    /// Class SdkController. Base controller for SDK endpoints.
    /// </summary>
    [ApiController]
    [SwaggerOperationOrder(From = SwaggerOperationOrderAttribute.OperationFrom.Sdk,
        OperationTags = new[] {
            SwaggerSdkConstants.ABOUT
        })]
    public class SdkController : ControllerBase { }
}
