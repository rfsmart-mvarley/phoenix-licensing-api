using Microsoft.AspNetCore.Mvc;
using Rfsmart.Phoenix.Licensing.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rfsmart.Phoenix.Licensing.Web.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FeatureTrackingController( ) : ControllerBase
    {
        // GET: api/<FeatureTrackingController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<FeatureTrackingController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<FeatureTrackingController>
        [HttpPost]
        public void Post([FromBody] AssignFeatureRequest value)
        {

        }

        // PUT api/<FeatureTrackingController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FeatureTrackingController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
