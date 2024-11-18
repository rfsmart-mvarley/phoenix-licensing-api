using Microsoft.AspNetCore.Mvc;
using Rfsmart.Phoenix.Licensing.Attributes;
using Rfsmart.Phoenix.Licensing.Interfaces;
using Rfsmart.Phoenix.Licensing.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Rfsmart.Phoenix.Licensing.Web.Controllers.Application
{
    [Route("[controller]")]
    [ApiController]
    [ValidateTenantContext]
    public class FeatureTrackingController(IFeatureTrackingService featureTrackingService) : ControllerBase
    {
        [HttpPost]
        public async Task<ActionResult<FeatureTrackingByUserResponse>> Post([FromBody] FeaturesRequest request)
        {
            return Ok(await featureTrackingService.AssignFeaturesToUser(request));
        }

        [HttpDelete]
        public async Task<ActionResult<FeatureTrackingByUserResponse>> Delete([FromQuery] FeaturesRequest request)
        {
            try
            {
                return Ok(await featureTrackingService.UnassignFeaturesFromUser(request));
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
                return Ok(await featureTrackingService.GetConsumption());
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

        [HttpGet("graph")]
        [Produces("image/png", "application/json")]
        public async Task<ActionResult<IEnumerable<FeatureTrackingRecord>>> GetGraph()
        {
            try
            {
                var resp = await featureTrackingService.GetAll();

                var client = new HttpClient();

                var url = GenerateUrl(resp);

                var l = url.Length;

                while (url.Length > 2083)
                {
                    resp = resp.Take(resp.Count() - 10);

                    url = GenerateUrl(resp);
                }

                var graph = await client.GetAsync(url);

                Byte[] b = await graph.Content.ReadAsByteArrayAsync();
                return File(b, "image/png");
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

        private static string GenerateUrl(IEnumerable<FeatureTrackingRecord> resp)
        {
            var colorQueue = new Queue<string>([
                                "rgb(255, 99, 132)",
                    "rgb(54, 162, 235)",
                    "rgb(50, 168, 82",
                ]);

            var chartObject = new ChartObject
            {
                Type = ChartType.line,
                Data = new ChartData
                {
                    Labels = resp.Select(x => x.Created.ToString()).Distinct().ToArray(),
                    DataSets = resp.GroupBy(x => x.FeatureName).Select(x =>
                    {
                        var color = colorQueue.Dequeue();

                        return new ChartDataSet
                        {
                            Label = x.Key,
                            Data = x.Select(d => d.UserCount).ToArray(),
                            BorderColor = color,
                            BackgroundColor = color,
                        };
                    }).ToArray(),
                    Options = new ChartOptions
                    {
                        Title = new ChartTitleOption
                        {
                            Text = "License Consumption Over Time"
                        }
                    }
                }
            };

            var jString = JsonSerializer.Serialize(chartObject, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            return string.Format("https://quickchart.io/chart?c={0}", jString);
        }
    }
}
