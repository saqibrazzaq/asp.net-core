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
using Rocky_DataAccess.Repository.IRepository;

namespace Rocky.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _repoProduct;
        private readonly ICategoryRepository _repoCategory;

        public HomeController(ILogger<HomeController> logger, IProductRepository repoProduct,
            ICategoryRepository repoCategory)
        {
            _logger = logger;
            _repoProduct = repoProduct;
            _repoCategory = repoCategory;
        }

        public IActionResult Index()
        {
            // Load all categories and products
            var homeVM = new HomeVM() 
            { 
                Categories = _repoCategory.GetAll(),
                Products = _repoProduct.GetAll(includeProperties: "ApplicationType,Category")
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
                Product = _repoProduct.FirstOrDefault(t => t.Id == id, 
                    includeProperties: "ApplicationType,Category"),
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
