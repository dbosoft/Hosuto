using System.Diagnostics;
using Dbosoft.Hosuto.Samples.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dbosoft.Hosuto.Samples.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMessageDispatcher _messageDispatcher;

        public HomeController(IMessageDispatcher messageDispatcher)
        {
            _messageDispatcher = messageDispatcher;
        }

        public IActionResult Index()
        {
            _messageDispatcher.SendMessage(this, "Someone has opened Home page!! Good start!");

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            _messageDispatcher.SendMessage(this, "Someone has opened About page!! Great!");

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";
            _messageDispatcher.SendMessage(this, "Someone has opened Contact page!! Great!");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
