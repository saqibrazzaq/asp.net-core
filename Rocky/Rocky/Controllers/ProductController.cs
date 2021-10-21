using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize(Roles = WC.AdminRole)]
    public class ProductController : Controller
    {
        // Db reference
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment web;
        public ProductController(ApplicationDbContext dbContext, IWebHostEnvironment webHost)
        {
            db = dbContext;
            web = webHost;
        }

        // Get - List all products
        public IActionResult Index()
        {
            // Get all products from db
            var products = db.Products
                .Include(x => x.ApplicationType)
                .Include(x => x.Category);
            return View(products);
        }

        // Get - Insert or Update
        public IActionResult InsertUpdate(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = db.Categories.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
                ApplicationSelectList = db.ApplicationTypes.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            
            // Load product if id is passed
            if (id == null)
            {
                return View(productVM);
            }
            else
            {
                // Get product from db
                productVM.Product = db.Products.Find(id);
                if (productVM.Product == null)
                {
                    return NotFound();
                }
                // Pass product to the view
                return View(productVM);
            }
        }

        // Post - Create update in db
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult InsertUpdate(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                // Get files
                var files = HttpContext.Request.Form.Files;
                string webRootPath = web.WebRootPath;

                // If product id is 0, create mode
                if (productVM.Product.Id == 0)
                {
                    // Create
                    string folder = webRootPath + WC.ImagePath;
                    string filename = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);
                    string file = Path.Combine(folder, filename + extension);

                    // Save file to wwwroot folder
                    using (var fileStream = new FileStream(file, FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    // Save only image name in db
                    productVM.Product.Image = filename + extension;
                    db.Products.Add(productVM.Product);
                }
                else
                {
                    // Update
                    // If there is a new image
                    if (files.Count > 0)
                    {
                        // Replace image
                        string folder = web.WebRootPath + WC.ImagePath;
                        string filename = Guid.NewGuid().ToString() + Path.GetExtension(files[0].FileName);

                        // Save new image in folder
                        using (var fileStream = new FileStream(Path.Combine(folder, filename), FileMode.Create))
                        {
                            files[0].CopyTo(fileStream);
                        }
                        // Delete old image from folder
                        string oldFile = Path.Combine(folder, productVM.Product.Image);
                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);
                        // Save new image in db
                        productVM.Product.Image = filename;
                    }
                    db.Products.Update(productVM.Product);
                    
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            // Load application type and category
            productVM.CategorySelectList = db.Categories.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Id.ToString()
            });
            productVM.ApplicationSelectList = db.ApplicationTypes.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            });
            return View(productVM);
        }

        // Get - Delete form
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();
            // Find in db
            var product = db.Products.Include(x => x.Category).First(x => x.Id == id);
            // If product not found
            if (product == null)
                return NotFound();
            // Pass product to the view
            return View(product);
        }

        // Post - Delete product from db
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(Product product)
        {
            // Delete file from folder
            string file = web.WebRootPath + WC.ImagePath + product.Image;
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);

            // Delete the product from db
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
