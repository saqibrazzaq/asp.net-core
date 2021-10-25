using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ApplicationTypeController : Controller
    {
        // Get db reference
        private readonly IApplicationTypeRepository _repo;
        public ApplicationTypeController(IApplicationTypeRepository repo)
        {
            _repo = repo;
        }
        // Get - Load all application types
        public IActionResult Index()
        {
            // Get all application types from db
            var applicationTypes = _repo.GetAll();
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
                _repo.Add(appType);
                _repo.Save();
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
            var appType = _repo.Find(id.GetValueOrDefault());
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
                _repo.Update(appType);
                _repo.Save();
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
            var appType = _repo.Find(id.GetValueOrDefault());
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
            _repo.Remove(appType);
            _repo.Save();
            return RedirectToAction("Index");
        }
    }
}
