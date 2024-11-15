using Microsoft.AspNetCore.Mvc;
using Rfsmart.Phoenix.Licensing.Attributes;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;

namespace Rfsmart.Phoenix.Licensing.Web.Controllers.Application
{
    [Route("[controller]")]
    [ApiController]
    [ValidateTenantContext]
    public class FeatureIssueController(IFeatureIssueService featureIssueService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] FeatureIssueRequest request)
        {
            await featureIssueService.IssueFeature(request);

            return Ok(request);
        }

        [HttpGet("{featureName}/latest")]
        public async Task<ActionResult> Get([FromRoute] string featureName)
        {
            var resp = await featureIssueService.GetCurrentFeatureIssuance(featureName);

            return Ok(resp);
        }

        [HttpGet("{featureName}/all")]
        public async Task<ActionResult> GetAll([FromRoute] string featureName)
        {
            var resp = await featureIssueService.GetAllFeatureIssuances(featureName);

            return Ok(resp);
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var resp = await featureIssueService.GetAllFeatureIssuances();

            return Ok(resp);
        }
    }
}
