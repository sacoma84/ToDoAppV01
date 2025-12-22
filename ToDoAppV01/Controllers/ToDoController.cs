using Microsoft.AspNetCore.Mvc;

namespace ToDoAppV01.Controllers
{
    public class ToDoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
