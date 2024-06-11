using Microsoft.AspNetCore.Mvc;

namespace ChatApp_BE.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}