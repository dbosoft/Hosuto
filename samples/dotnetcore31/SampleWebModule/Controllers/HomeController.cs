using System.Diagnostics;
using Dbosoft.Hosuto.Sample.Models;
using Dbosoft.Hosuto.Samples;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Dbosoft.Hosuto.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMessageDispatcher _messageDispatcher;

        public HomeController(ILogger<HomeController> logger, IMessageDispatcher messageDispatcher)
        {
            _logger = logger;
            _messageDispatcher = messageDispatcher;
        }

        public IActionResult Index()
        {
            _messageDispatcher.SendMessage(this, "Someone has opened Home page!! Good start!");
            return View();
        }

        public IActionResult Privacy()
        {
            _messageDispatcher.SendMessage(this, "Someone has opened Privacy page!! Great!");

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
