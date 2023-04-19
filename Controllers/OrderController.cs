﻿using FPTBook_v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FPTBook_v3.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;


        public OrderController(ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
             IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        [Authorize(Roles = "User")]
        [Route("/User/UserOrders")]
        public async Task<IEnumerable<Order>> UserOrders()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User is not logged-in");
            var orders = await _db.Orders
                            .Include(x => x.OrderDetail)
                            .ThenInclude(x => x.Book)
                            .ThenInclude(x => x.category)
                            .Where(a => a.UserId == userId)
                            .ToListAsync();
            return orders;
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

        [Authorize(Roles = "User")]
        [Route("/User/UserOrders/OrderDetail")]
        public async Task<IActionResult> OrderDetail()
        {
            var orders = await UserOrders();
            return View(orders);
        }

        [Authorize(Roles = "Owner")]
        [Route("Owner/GetOrder")]
        public async Task<IActionResult> GetOrder()
        {
            var orders = await _db.Orders
                            .Include(x => x.ApplicationUsers)
                            .Include(x => x.OrderDetail)
                            .ThenInclude(x => x.Book)
                            .ThenInclude(x => x.category)

                            .ToListAsync();
            return View(orders);
        }
    }
}
