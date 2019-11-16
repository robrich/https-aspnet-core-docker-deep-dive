using System;
using System.Collections.Generic;
using System.Linq;
using HTTPSPlayground.API.Models;
using HTTPSPlayground.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HTTPSPlayground.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IEndpointDetailsService endpointDetailsService;

        public ValuesController(IEndpointDetailsService endpointDetailsService)
        {
            this.endpointDetailsService = endpointDetailsService ?? throw new ArgumentNullException(nameof(endpointDetailsService));
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            EndpointDetailsViewModel model = this.endpointDetailsService.HackEndpointDetails();
            List<string> thumbprints = (
                from t in model.EndpointDetails
                where t.Certificate?.Thumbprint != null
                select t.Certificate?.Thumbprint
            ).Distinct().ToList();
            return thumbprints;
            //return new string[] { "value1", "value2" };
        }

        /*
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
        */

    }
}
