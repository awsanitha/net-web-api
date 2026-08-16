using Net.Web.Api.Sdk.Controllers.Common;
using Net.Web.Api.Sdk.Documentation.Attributes;
using Net.Web.Api.Sdk.Web.Examples.Classes.Constants;

namespace Net.Web.Api.Sdk.Web.Examples.Controllers.Common
{
    /// <summary>
    /// Base controller for example API controllers.
    /// </summary>
    [SwaggerOperationOrder(
        From = SwaggerOperationOrderAttribute.OperationFrom.Application,
        OperationTags = new[] {
            ExampleControllerGroups.SECURITY,
            ExampleControllerGroups.OTHER
        })]
    public class ExampleController : SdkController
    {
        #region Constants

        /// <summary>Route prefix for example endpoints.</summary>
        protected const string ROUTE_PREFIX = "example/";

        #endregion
    }
}
