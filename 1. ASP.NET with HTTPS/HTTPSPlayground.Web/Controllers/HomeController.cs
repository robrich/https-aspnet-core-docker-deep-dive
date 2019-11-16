using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HTTPSPlayground.Web.Models;
using HTTPSPlayground.Web.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HTTPSPlayground.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IEndpointDetailsService endpointDetailsService;
        private readonly ICallApiService callApiService;

        public HomeController(IEndpointDetailsService endpointDetailsService, ICallApiService callApiService)
        {
            this.endpointDetailsService = endpointDetailsService ?? throw new ArgumentNullException(nameof(endpointDetailsService));
            this.callApiService = callApiService ?? throw new ArgumentNullException(nameof(callApiService));
        }

        public IActionResult Index()
        {
            ICollection<string> model = endpointDetailsService.GentlyGetEndpointAddresses();
            return View(model ?? new List<string>());
        }

        public IActionResult Endpoints()
        {
            EndpointDetailsViewModel model = endpointDetailsService.HackEndpointDetails();
            return View(model);
        }

        public async Task<IActionResult> API()
        {
            string url = "https://localhost:4001/api/values";
            //string url = "https://self-signed.badssl.com/";
            //string url = "https://api:4001/api/values";
            var model = await callApiService.MakeCall(url);
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
