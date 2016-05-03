using Microsoft.AspNetCore.Mvc;

namespace MinifiersTagHelpers
{
    public class HomeController : Controller
    {
        [HttpGet("/")]
        public IActionResult Index() => View();
    }
}