﻿using Microsoft.AspNetCore.Mvc;

namespace WebFlow.Sample.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}