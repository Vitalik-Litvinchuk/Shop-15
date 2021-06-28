using Microsoft.AspNetCore.Mvc;
using Shop_15.Data;
using Shop_15.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_15.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Get
        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _db.Categories;
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View();
        }

        // Get
        public IActionResult Delete(int? id)
        {
            if (id != null || id != 0)
            {
                Category category = _db.Categories.FirstOrDefault(c => c.Id == id);
                if (category != null)
                    return View(category);
            }
            return NotFound();
        }

        // Get
        public IActionResult Edit(int? id)
        {
            if (id != null || id != 0)
            {
                Category category = _db.Categories.FirstOrDefault(c => c.Id == id);
                if (category != null)
                    return View(category);
            }
            return NotFound();
        }

        // Post 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            Category category = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _db.Categories.Remove(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return NotFound();
        }


        // Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }
    }
}
