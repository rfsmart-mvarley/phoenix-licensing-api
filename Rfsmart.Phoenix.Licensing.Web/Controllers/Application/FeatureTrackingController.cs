using Microsoft.AspNetCore.Mvc;
using Rfsmart.Phoenix.Licensing.Attributes;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rfsmart.Phoenix.Licensing.Web.Controllers.Application
{
    [Route("[controller]")]
    [ApiController]
    [ValidateTenantContext]
    public class FeatureTrackingController(IFeatureTrackingService featureTrackingService) : ControllerBase
    {
        // POST api/<FeatureTrackingController>
        [HttpPost]
        public async void Post([FromBody] AssignFeatureRequest request)
        {
            await featureTrackingService.AssignFeaturesToUser(request);
        }
    }
}
