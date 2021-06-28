using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop_15.Data;
using Shop_15.Models;
using Shop_15.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_15.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnviroment;

        public ProductController(ApplicationDbContext db, IWebHostEnvironment webHostEnviroment)
        {
            _db = db;
            _webHostEnviroment = webHostEnviroment;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _db.Products.Include(p => p.Category);
            return View(productList);
        }

        // Get
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategorySelectList = _db.Categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };

            if (id == null)
                return View(productVM);
            else
            {
                productVM.Product = _db.Products.FirstOrDefault(p => p.Id == id);
                if (productVM.Product == null)
                    return NotFound();
                return View(productVM);
            }
        }

        // Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                string webRootPath = _webHostEnviroment.WebRootPath;

                string upload = webRootPath + ENV.productImage;
                string fileName = Guid.NewGuid().ToString();
                string extension = "";
                if (files.Count > 0)
                {
                    extension = Path.GetExtension(files[0].FileName);
                    using (FileStream fs = new FileStream(Path.Combine(upload, fileName + extension), FileMode.Create))
                        files[0].CopyTo(fs);
                }

                if (productVM.Product.Id == 0)
                {
                    productVM.Product.Image = fileName + extension;
                    _db.Products.Add(productVM.Product);
                    _db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    var formObject = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == productVM.Product.Id);
                    if (files.Count > 0 && formObject.Image != null)
                    {
                        var oldFile = Path.Combine(upload, formObject.Image);
                        if (System.IO.File.Exists(oldFile))
                            System.IO.File.Delete(oldFile);
                        productVM.Product.Image = fileName + extension;
                    }
                    else
                        productVM.Product.Image = formObject.Image;
                    _db.Products.Update(productVM.Product);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            productVM.CategorySelectList = _db.Categories.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            return View(productVM);
        }

        // Get
        public IActionResult Delete(int? id)
        {
            if (id != null || id != 0)
            {
                Product product = _db.Products.FirstOrDefault(p => p.Id == id);
                if (product != null)
                    return View(product);
            }
            return NotFound();
        }

        // Post 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            Product product = _db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                if (product.Image != null)
                    System.IO.File.Delete(Path.Combine(_webHostEnviroment.WebRootPath + ENV.productImage, product.Image));

                _db.Products.Remove(product);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
    }
}
