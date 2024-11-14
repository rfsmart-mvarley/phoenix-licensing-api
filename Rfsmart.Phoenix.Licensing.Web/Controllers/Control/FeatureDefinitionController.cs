using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rfsmart.Phoenix.Licensing.Web.Controllers.Control
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class FeatureDefinitionController(IFeatureDefinitionRepository featureDefinitionRepository) : ControllerBase
    {
        // GET api/<FeatureTrackingController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FeatureDefinition?>> Get(string id)
        {
            var resp = await featureDefinitionRepository.Get(id);

            if (resp is null)
            {
                return NotFound();
            }

            return Ok(resp);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] FeatureDefinition request)
        {
            await featureDefinitionRepository.Create(request);

            return Ok(request);
        }
    }
}
