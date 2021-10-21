using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        // Get db reference
        private readonly ApplicationDbContext db;
        public ApplicationTypeController(ApplicationDbContext dbContext)
        {
            db = dbContext;
        }
        // Get - Load all application types
        public IActionResult Index()
        {
            // Get all application types from db
            var applicationTypes = db.ApplicationTypes;
            return View(applicationTypes);
        }

        // Get - Create form
        public IActionResult Create()
        {
            return View();
        }

        // Post - Create application type in db
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationType appType)
        {
            if (ModelState.IsValid)
            {
                db.ApplicationTypes.Add(appType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        // Get - Edit form
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            // Get application type from db
            var appType = db.ApplicationTypes.Find(id);
            if (appType == null)
                return NotFound();
            // Pass application type to the view
            return View(appType);
        }

        // Post - save application type in db
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ApplicationType appType)
        {
            if (ModelState.IsValid)
            {
                db.Update(appType);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        // Get - Delete form
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            // Get application type from db
            var appType = db.ApplicationTypes.Find(id);
            if (appType == null)
                return NotFound();
            // Pass the application type to the view
            return View(appType);
        }

        // Post - Delete application type from db
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(ApplicationType appType)
        {
            // Delete from db
            db.Remove(appType);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
