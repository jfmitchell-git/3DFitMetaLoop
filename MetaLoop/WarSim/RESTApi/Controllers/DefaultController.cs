using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace RESTApi.Controllers
{
    [Route("api/{method}/{param1}/{param2}")]
    [Route("api/{method}/{param1}")]
    [Route("api/{method}")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public string Get(string method, string param1, string param2)
        {
            return Request.RouteValues.ElementAt(0).ToString();
        }

    }
}