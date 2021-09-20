using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    public class CategoryController : Controller
    {
        // Get db instance
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }
        // Default - Get all categories
        public IActionResult Index()
        {
            // Get list of categories
            IEnumerable<Category> objList = _db.Category;
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
                _db.Category.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }
    }
}
