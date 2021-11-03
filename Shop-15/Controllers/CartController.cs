using Microsoft.AspNetCore.Mvc;
using Shop_15.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Shop_15.Models;
using Shop_15.Services;
using Microsoft.EntityFrameworkCore;
using Shop_15.Models.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text;

namespace Shop_15.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        public CartController(ApplicationDbContext db, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart).Count() > 0)
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessionCart);

            List<Product> p = new List<Product>();
            foreach (var item in shoppingCartList)
                foreach (var item2 in _db.Products.Include(p => p.Category))
                    if (item.ProductId == item2.Id)
                        p.Add(item2);
            return View(p);
        }

        public IActionResult RemoveFromCart(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart).Count() > 0)
            {
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessionCart);
            }

            var removeItem = shoppingCartList.SingleOrDefault(u => u.ProductId == id);
            if (removeItem != null)
                shoppingCartList.Remove(removeItem);

            HttpContext.Session.Set(ENV.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart).Count() > 0)
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessionCart);

            DetailVM detailVM = new DetailVM()
            {
                Product = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id),
                InCart = false
            };

            foreach (var item in shoppingCartList)
                if (item.ProductId == id)
                    detailVM.InCart = true;

            return View(detailVM);
        }

        public IActionResult Order()
        {
            var claimsIdntity = (ClaimsIdentity)User.Identity;
            var clime = claimsIdntity.FindFirst(ClaimTypes.NameIdentifier);

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart) != null &&
                HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart).Count() > 0)
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessionCart);

            List<int> productInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> productList = _db.Products.Where(i => productInCart.Contains(i.Id));

            ProductUserVM ProductUserVM = new ProductUserVM()
            {
                AppUser = _db.AppUsers.FirstOrDefault(i => i.Id == clime.Value),
                ProductList = productList.ToList()
            };

            return View(ProductUserVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Send email info about order
        public async Task<IActionResult> OrderPost(ProductUserVM productUserVM)
        {
            var pathTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                + "templates" + Path.DirectorySeparatorChar.ToString() + "OrderConfirmation.html";

            var subject = "New order";
            string HtmlBody = "";

            using (StreamReader sr = System.IO.File.OpenText(pathTemplate))
                HtmlBody = sr.ReadToEnd();

            StringBuilder productSB = new StringBuilder();
            productSB.Append("<br>");
            foreach (var item in productUserVM.ProductList)
                productSB.Append($"<div style='border: 2px solid black; width: max-content; padding: 1% 2%; margin: 1% 3%; font-size: 1.2rem'>{item.Name} <br> Price: {item.Price}</div>");

            string message = string.Format(
                HtmlBody,
                "<h1 style='text-align: center'>" + productUserVM.AppUser.Name + " " + productUserVM.AppUser.Surname + "<h1>",
                productSB.ToString()
                );

            await _emailSender.SendEmailAsync(ENV.AdminEmail, subject, message);
            return RedirectToAction(nameof(OrderConfirmation));
        }

        public IActionResult OrderConfirmation()
        {
            HttpContext.Session.Clear();
            return View();
        }
    }
}
