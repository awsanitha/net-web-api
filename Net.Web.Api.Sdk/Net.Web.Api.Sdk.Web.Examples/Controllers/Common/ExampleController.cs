using Microsoft.AspNetCore.Mvc;
using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.Common
{
    /// <summary>
    /// Base controller for example endpoints.
    /// </summary>
    [SwaggerOperationOrder(From = SwaggerOperationOrderAttribute.OperationFrom.Application,
        OperationTags = new[] { ExampleControllerGroups.SECURITY, ExampleControllerGroups.OTHER })]
    public class ExampleController : SdkController
    {
        protected const string ROUTE_PREFIX = "example/";
    }
}
