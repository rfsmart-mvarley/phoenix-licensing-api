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
    public class FeatureIssueController(IFeatureIssueService featureIssueService) : ControllerBase
    {
        // POST api/<FeatureTrackingController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] FeatureIssueRequest request)
        {
            await featureIssueService.IssueFeature(request);

            return Ok(request);
        }
    }
}
