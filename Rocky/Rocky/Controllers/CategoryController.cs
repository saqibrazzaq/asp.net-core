using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky_DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class CategoryController : Controller
    {
        // Get db instance
        private readonly ICategoryRepository _categoryRepo;

        public CategoryController(ICategoryRepository catetoryRepo)
        {
            _categoryRepo = catetoryRepo;
        }
        // Default - Get all categories
        public IActionResult Index()
        {
            // Get list of categories
            IEnumerable<Category> objList = _categoryRepo.GetAll();
            return View(objList);
        }

        // GET - Create Form
        public IActionResult Create()
        {
            return View();
        }

        // POST - Create Form submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Add category in db
                _categoryRepo.Add(category);
                _categoryRepo.Save();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null|| id == 0)
            {
                return NotFound();
            }

            // Get record from database
            var category = _categoryRepo.Find(id.GetValueOrDefault());
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _categoryRepo.Update(category);
                _categoryRepo.Save();
                return RedirectToAction("Index");
            }
            return View();
        }

        // Get - Delete
        public IActionResult Delete(int? id)
        {
            // Find the id in database and pass it to delete confirmation page
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _categoryRepo.Find(id.GetValueOrDefault());
            // If id not found
            if (category == null)
            {
                return NotFound();
            }
            // Pass category to the view
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Category category)
        {
            if (category == null)
            {
                return NotFound();
            }
            // Delete from database
            _categoryRepo.Remove(category);
            _categoryRepo.Save();
            return RedirectToAction("Index");
        }
    }
}
