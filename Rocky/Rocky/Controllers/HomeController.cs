using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Rocky.Utility;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            // Load all categories and products
            var homeVM = new HomeVM() 
            { 
                Categories = _db.Categories,
                Products = _db.Products.Include(t => t.ApplicationType).Include(t => t.Category)
            };
            return View(homeVM);
        }

        // Get - Details page
        public IActionResult Details(int id)
        {
            List<ShoppingCart> carts = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                carts = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            // Initialize VM
            var detailsVM = new DetailsVM()
            {
                Product = _db.Products
                    .Include(t => t.Category)
                    .Include(t => t.ApplicationType)
                    .Where(t => t.Id == id)
                    .FirstOrDefault(),
                ExistsInCart = false
            };

            foreach (var item in carts)
            {
                if (item.ProductId == id)
                    detailsVM.ExistsInCart = true;
            }

            return View(detailsVM);
        }

        // Post - Add to cart
        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCart> carts = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                carts = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }
            carts.Add(new ShoppingCart { ProductId = id });
            HttpContext.Session.Set(WC.SessionCart, carts);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> carts = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null
                && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0)
            {
                carts = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            var itemToRemove = carts.SingleOrDefault(x => x.ProductId == id);
            if (itemToRemove != null)
                carts.Remove(itemToRemove);

            HttpContext.Session.Set(WC.SessionCart, carts);
            return RedirectToAction("Index");
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
