using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using ToDoAppV01.Data;
using ToDoAppV01.Models;

namespace ToDoAppV01.Controllers
{
    public class ToDoController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly UserManager<IdentityUser> _userManager;

        // Konstruktor
        public ToDoController(ApplicationDbContext applicationDbContext, UserManager<IdentityUser> userManager)
        {
            _applicationDbContext = applicationDbContext;
            _userManager = userManager;
        } // Konstruktor - ENDE

        /// <summary>
        /// View mit allen ToDoListen des angemeldeten Benutzers anzeigen
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            #region UserId holen
            if (!User.Identity?.IsAuthenticated ?? true)
                return Challenge(); // oder RedirectToAction("Login")

            // 1) Direkt die Id als string bekommen:
            var userId = _userManager.GetUserId(User);

            // oder 2) User-Objekt und daraus Id:
            // var user = await _userManager.GetUserAsync(User);
            // var userId = user?.Id;
            #endregion UserId holen

            List<ToDoList?> toDoLists;
            if (!userId.IsNullOrEmpty())
            {
                toDoLists = _applicationDbContext.ToDoLists.Where(x => x.UserId.Contains(userId)).Include(l => l.Items).ToList();
                if (toDoLists != null)
                {
                    ViewBag.ToDoLists = toDoLists;
                }

            }

            return View();
        }


        [HttpPost]
        public IActionResult CreateNewToDoList(string toDoListTitle, string toDoListDesc)
        {
            Trace.WriteLine($"CreateNewToDoList called: toDoListTitle: {toDoListTitle} | toDoListDesc: {toDoListDesc}");
            var userId = _userManager.GetUserId(User);

            ToDoList toDoList = new ToDoList()
            {
                ListTitle = toDoListTitle,
                ListDescription = toDoListDesc,
                UserId = userId,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };

            _applicationDbContext.ToDoLists.Add(toDoList);
            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Create a new ToDoItem in the specified ToDoList.
        /// </summary>
        /// <param name="toDoListId"></param>
        /// <param name="toDoText"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult CreateNewToDoItem(int toDoListId, string toDoText)
        {

            Trace.WriteLine($"CreateNewToDoItem called: {toDoListId} - {toDoText}");
            ToDoItem newToDoItem = new ToDoItem()
            {
                ToDoListId = toDoListId,
                ItemTitle = toDoText,
                IsCompleted = false,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            };
            _applicationDbContext.ToDoItems.Add(newToDoItem);
            _applicationDbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Delete a ToDoItem in the specified ToDoList.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult DeleteToDoItem(int id)
        {
            var toDoItem = _applicationDbContext.ToDoItems.Find(id);
            if (toDoItem != null)
            {
                _applicationDbContext.ToDoItems.Remove(toDoItem);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult DeleteToDoList(int id)
        {
            var toDoList = _applicationDbContext.ToDoLists.Include(l => l.Items).FirstOrDefault(l => l.Id == id);
            if (toDoList != null)
            {
                // Zuerst alle zugehörigen ToDoItems löschen
                _applicationDbContext.ToDoItems.RemoveRange(toDoList.Items);
                // Dann die ToDoList löschen
                _applicationDbContext.ToDoLists.Remove(toDoList);
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Edit ToDoList Title und Description
        /// </summary>
        /// <param name="toDoListTitleId"></param>
        /// <param name="tbxToDoTitle"></param>
        /// <param name="tbxToDoDescription"></param>
        /// <returns></returns>
        public IActionResult EditToDoList(int toDoListTitleId, string tbxToDoTitle, string tbxToDoDescription)
        {
            Trace.WriteLine($"toDoItemId: {toDoListTitleId} | txbToDoTitle: {tbxToDoTitle} | txbToDoDescription: {tbxToDoDescription}");
            var todoListDs = _applicationDbContext.ToDoLists.Find(toDoListTitleId);
            if (todoListDs != null)
            {
                todoListDs.ListTitle = tbxToDoTitle;
                todoListDs.ListDescription = tbxToDoDescription;
                todoListDs.ModifiedAt = DateTime.Now;
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// ToDoItem bearbeiten
        /// </summary>
        /// <param name="toDoItemId"></param>
        /// <param name="txbToDoText"></param>
        /// <returns></returns>
        public IActionResult EditToDoItem(int toDoItemId, string txbToDoText)
        {
            var toDoItem = _applicationDbContext.ToDoItems.Find(toDoItemId);
            if (toDoItem != null)
            {
                toDoItem.ItemTitle = txbToDoText;
                toDoItem.ModifiedAt = DateTime.Now;
                _applicationDbContext.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// ToDoItem Status aktualisieren (abgeschlossen/nicht abgeschlossen)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus([FromBody] UpdateToDoStatusDto dto)
        {
            // Beispiel
            var todo = _applicationDbContext.ToDoItems.Find(dto.Id);
            if (todo == null)
                return NotFound();

            todo.IsCompleted = dto.IsDone;
            _applicationDbContext.SaveChanges();

            return Ok();
        }
    }
    #region Innere Klassen
    public class UpdateToDoStatusDto
    {
        public int Id { get; set; }
        public bool IsDone { get; set; }
    }
    #endregion Innere Klassen
}
