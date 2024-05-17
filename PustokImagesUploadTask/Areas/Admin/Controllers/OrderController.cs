using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PustokImagesUploadTask.Models;

namespace PustokImagesUploadTask.Areas.Admin.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin")]
    [Area("Admin")]
    public class OrderController : Controller
    {


        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            List<Order> orders = _context.Orders.ToList();
            if (orders.Count == 0)
            {
                return RedirectToAction("dashboard", "admin");
            }
            return View(orders);
        }
        public IActionResult Detail(int id)
        {
            Order order = _context.Orders.Include(x => x.OrderItems).FirstOrDefault(x => x.Id == id);
            if (order == null) { return View("Error"); }
            return View(order);
        }

        public IActionResult Accept(int id) 
        {
            Order order = _context.Orders.FirstOrDefault(x => x.Id == id);

            if(order== null) { return View("Error"); }

            order.Status = Enums.OrderStatus.Accepted;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Reject(int id) 
        {
            Order order = _context.Orders.FirstOrDefault(x => x.Id == id);
            if (order == null) { return View("Error"); }

            order.Status = Enums.OrderStatus.Rejected;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
