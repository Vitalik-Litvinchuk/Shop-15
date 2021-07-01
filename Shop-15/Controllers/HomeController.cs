﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shop_15.Data;
using Shop_15.Models;
using Shop_15.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Shop_15.Controllers
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
            HomeVM homeVM = new HomeVM()
            {
                Products = _db.Products.Include(p => p.Category),
                Categories = _db.Categories
            };
            return View(homeVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Get
        public IActionResult Details(int id)
        {
            DetailVM detailVM = new DetailVM() { Product = _db.Products.Include(p => p.Category).FirstOrDefault(p => p.Id == id) };
            return View(detailVM);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
