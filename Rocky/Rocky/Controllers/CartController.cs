using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _web;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }
        public CartController(ApplicationDbContext db, IWebHostEnvironment web,
            IEmailSender sender)
        {
            _db = db;
            _web = web;
            _emailSender = sender;
        }
        public IActionResult Index()
        {
            List<ShoppingCart> cartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null)
            {
                // Session exists
                cartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            // Get product ids from session
            var prodIds = cartList.Select(x => x.ProductId).ToList();

            // Get the products from db, which are in cart
            var products = _db.Products.Where(x => prodIds.Contains(x.Id));

            // Return products to the view
            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult IndexPost()
        {
            return RedirectToAction("Summary");
        }

        // Get - Show summary - product list and user info
        public IActionResult Summary()
        {
            // Get user id
            var userId = User.FindFirstValue(ClaimTypes.Name);

            // Initialize empty cart
            var cartItems = new List<ShoppingCart>();
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null)
                cartItems = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

            // Get products list from cart
            var productIds = cartItems.Select(x => x.ProductId).ToList();
            var products = _db.Products.Where(x => productIds.Contains(x.Id)).ToList();

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Email == userId),
                Products = products
            };

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVM)
        {
            var pathToTemplate = _web.WebRootPath + Path.DirectorySeparatorChar +
                "templates" + Path.DirectorySeparatorChar + "Inquiry.html";
            var subject = "New Inquiry";
            var htmlBody = "";
            using (var sr = System.IO.File.OpenText(pathToTemplate))
            {
                htmlBody = sr.ReadToEnd();
            }

            StringBuilder builder = new StringBuilder();
            foreach (var prod in productUserVM.Products)
            {
                builder.Append($" - Name: {prod.Name} - ID: {prod.Id}");
            }

            string messageBody = string.Format(htmlBody, 
                productUserVM.ApplicationUser.FullName,
                productUserVM.ApplicationUser.Email,
                productUserVM.ApplicationUser.PhoneNumber,
                builder.ToString());

            await _emailSender.SendEmailAsync(WC.AdminEmail, subject, messageBody);

            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation()
        {
            // Clear session
            HttpContext.Session.Clear();

            return View();
        }

        // Get - Remove from cart
        public IActionResult Remove(int? id)
        {
            // If id is null
            if (id == null || id == 0)
                return NotFound();

            // Initialize empty cart
            var cartItems = new List<ShoppingCart>();
            // Get from session
            if (HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart) != null)
            {
                cartItems = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);

                // Find id in cart
                var item = cartItems.FirstOrDefault(x => x.ProductId == id);
                // Remove from cart
                if (item != null)
                {
                    cartItems.Remove(item);
                    // Update session
                    HttpContext.Session.Set<List<ShoppingCart>>(WC.SessionCart, cartItems);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
