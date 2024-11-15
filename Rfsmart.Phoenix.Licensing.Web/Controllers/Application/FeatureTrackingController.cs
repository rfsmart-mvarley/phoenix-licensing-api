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
        [HttpPost]
        public async Task<ActionResult<AssignFeatureRequest>> Post([FromBody] AssignFeatureRequest request)
        {
            return Ok(await featureTrackingService.AssignFeaturesToUser(request));
        }

        [HttpGet("byUser")]
        public async Task<ActionResult<FeatureTrackingByUserResponse>> Get([FromQuery] FeatureTrackingByUserRequest request)
        {
            try
            {
                return Ok(await featureTrackingService.Get(request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = 500,
                };
            }
        }

        [HttpGet("byFeature")]
        public async Task<ActionResult<FeatureTrackingByFeatureRequest>> Get([FromQuery] FeatureTrackingByFeatureRequest request)
        {
            try
            {
                return Ok(await featureTrackingService.Get(request));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = 500,
                };
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FeatureTrackingRecord>>> Get()
        {
            try
            {
                return Ok(await featureTrackingService.Get());
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = 500,
                };
            }
        }
    }
}
