using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Common.Constants;
using Net.Web.Api.Sdk.Documentation.Attributes;

namespace Net.Web.Api.Sdk.Controllers.Common
{
    /// <summary>
    /// Base SDK controller.
    /// </summary>
    [SwaggerOperationOrder(From = SwaggerOperationOrderAttribute.OperationFrom.Sdk,
        OperationTags = new[] { SwaggerSdkConstants.ABOUT })]
    [ApiController]
    public class SdkController : ControllerBase { }
}
