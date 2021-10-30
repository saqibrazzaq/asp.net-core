using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky.Data;
using Rocky.Models;
using Rocky.Models.ViewModels;
using Rocky.Utility;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;
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
        private readonly IProductRepository _repoProduct;
        private readonly IApplicationUserRepository _repoUser;
        private readonly IInquiryHeaderRepository _repoInqHeader;
        private readonly IInquiryDetailRepository _repoInqDetail;
        private readonly IWebHostEnvironment _web;
        private readonly IEmailSender _emailSender;
        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }
        public CartController(IProductRepository repoProduct, 
            IWebHostEnvironment web,
            IEmailSender sender,
            IApplicationUserRepository repoUser,
            IInquiryHeaderRepository repoInqHeader,
            IInquiryDetailRepository repoInqDetail)
        {
            _repoProduct = repoProduct;
            _repoInqHeader = repoInqHeader;
            _repoInqDetail = repoInqDetail;
            _repoUser = repoUser;
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
            var products = _repoProduct.GetAll(x => prodIds.Contains(x.Id));

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
            var products = _repoProduct.GetAll(x => productIds.Contains(x.Id)).ToList();

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = _repoUser.FirstOrDefault(u => u.Email == userId),
                Products = products
            };

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SummaryPost(ProductUserVM productUserVM)
        {
            // Get logged in user id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

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

            // Create inquiry header
            var inqHeader = new InquiryHeader()
            {
                ApplicationUserId = claim.Value,
                FullName = productUserVM.ApplicationUser.FullName,
                Email = productUserVM.ApplicationUser.Email,
                PhoneNumber = productUserVM.ApplicationUser.PhoneNumber,
                InquiryDate = DateTime.Now
            };
            _repoInqHeader.Add(inqHeader);
            _repoInqHeader.Save();

            // Add inquiry detail
            foreach (var prod in productUserVM.Products)
            {
                var inqDetail = new InquiryDetail()
                {
                    InquiryHeaderId = inqHeader.Id,
                    ProductId = prod.Id
                };
                _repoInqDetail.Add(inqDetail);
            }
            _repoInqDetail.Save();

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
