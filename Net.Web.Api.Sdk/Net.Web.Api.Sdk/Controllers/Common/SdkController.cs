using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;

namespace Net.Web.Api.Sdk.Controllers.Common
{
    /// <summary>
    /// Base controller for all SDK controllers.
    /// </summary>
    [ApiController]
    [SwaggerOperationOrder(
        From = SwaggerOperationOrderAttribute.OperationFrom.Sdk,
        OperationTags = new[] { SwaggerSdkConstants.ABOUT })]
    public class SdkController : ControllerBase { }
}
