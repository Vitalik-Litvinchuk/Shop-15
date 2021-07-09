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

namespace Shop_15.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart).Count() > 0)
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessionCart);

            List<Product> p = new List<Product>();
            foreach (var item in shoppingCartList)
                foreach (var item2 in _db.Products.Include(p=>p.Category))
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
            {
                shoppingCartList.Remove(removeItem);
            }

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

        [HttpPost, ActionName("Details")]
        public IActionResult DetailsPost(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart) != null && HttpContext.Session.Get<IEnumerable<ShoppingCart>>(ENV.SessionCart).Count() > 0)
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(ENV.SessionCart);

            shoppingCartList.Add(new ShoppingCart { ProductId = id });
            HttpContext.Session.Set(ENV.SessionCart, shoppingCartList);
            return RedirectToAction("Index");
        }
    }
}
