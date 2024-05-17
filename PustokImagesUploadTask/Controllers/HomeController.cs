using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PustokImagesUploadTask.Models;
using PustokImagesUploadTask.ViewModels;

namespace PustokImagesUploadTask.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            HomeViewModel homeViewModel = new HomeViewModel()
            {
                FeaturedBooks = _context.Books.Where(x => x.IsFeatured == true).Include(x => x.Author).Include(x => x.Category).Include(x => x.BookImages).ToList(),
                NewBooks = _context.Books.Where(x => x.IsNew == true).Include(x => x.Author).Include(x => x.Category).Include(x => x.BookImages).ToList(),
                DiscountedBooks = _context.Books.Where(x => x.DiscountPrice > 0).Include(x => x.Author).Include(x => x.Category).Include(x => x.BookImages).ToList(),
                AllBooks = _context.Books.Include(x => x.Author).Include(x => x.Category).Include(x => x.BookImages).ToList(),
                Sliders = _context.Sliders.ToList(),
                Features = _context.Features.ToList()
            };
            return View(homeViewModel);
        }
        public IActionResult Detail(int id)
        {
            Book book = _context.Books.Include(x => x.Author).Include(x => x.Category).Include(x => x.BookImages).FirstOrDefault(x => x.Id == id);
            return View(book);
        }

        public async Task<IActionResult> Checkout()
        {
            List<BasketItemsViewModel> basketItemsList = new List<BasketItemsViewModel>();
            List<CheckOutViewModel> existitems = new List<CheckOutViewModel>();
            OrderViewModel orderViewModel = null;
            AppUser member = null;
            if (User.Identity.IsAuthenticated)
            {
                member = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            if (member != null)
            {
                var items = _context.BasketItems.Include(x=>x.Book).Where(x => x.AppUserId == member.Id && x.IsDeleted==false).ToList();
                foreach (var item in items)
                {
                    CheckOutViewModel checkOutViewModel = new CheckOutViewModel()
                    {
                        Book = _context.Books.FirstOrDefault(x=>x.Id==item.BookId),
                        Count = item.Count,
                    };
                    existitems.Add(checkOutViewModel);
                }
            }
            else
            {

                string basketitems = HttpContext.Request.Cookies["BasketItems"];
                if (basketitems is not null)
                {
                    basketItemsList = JsonConvert.DeserializeObject<List<BasketItemsViewModel>>(basketitems);
                    foreach (var item in basketItemsList)
                    {
                        CheckOutViewModel checkoutitem = new CheckOutViewModel()
                        {
                            Book = _context.Books.FirstOrDefault(x => x.Id == item.BookId),
                            Count = item.Count,
                        };
                        existitems.Add(checkoutitem);
                    }
                }
            }

            orderViewModel = new OrderViewModel()
            {
                CheckOutViewModels= existitems,
                FullName = member.FullName,
                Email= member.Email,
                PhoneNumber= member.PhoneNumber,
            };

            return View(orderViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Order(OrderViewModel orderVm)
        {
            if (!ModelState.IsValid) return View(nameof(Checkout));
            AppUser member = null;
            Order order = null;
            List<BasketItemsViewModel> basketitems = null;
            double totalprice = 0;

            if (User.Identity.IsAuthenticated)
            {
                member = await _userManager.FindByNameAsync(User.Identity.Name);
            }

            order = new Order
            {
                Address= orderVm.Address,
                Country= orderVm.Country,
                City= orderVm.City,
                Email = orderVm.Email,
                PhoneNumber= orderVm.PhoneNumber,
                FullName= orderVm.FullName,
                Note= orderVm.Note,
                ZipCode= orderVm.ZipCode,
                AppUserId = member?.Id,
                Status = Enums.OrderStatus.Pending
            };

            order.OrderItems = new List<OrderItem>();


            if (member != null)
            {
                var items = _context.BasketItems.Where(x=>x.AppUserId == member.Id && x.IsDeleted==false).ToList();

                foreach(var item in items)
                {
                    item.IsDeleted = true;
                    Book book = _context.Books.FirstOrDefault(x=>x.Id==item.BookId);
                    OrderItem orderItem = new OrderItem
                    {
                        BookId = book.Id,
                        CostPrice = book.CostPrice,
                        SalePrice= book.SalePrice*(1-book.DiscountPrice/100),
                        DiscountPrice= book.DiscountPrice,
                        Count=item.Count,
                        BookName = book.Name,
                        Order = order,
                    };
                    totalprice += orderItem.SalePrice * orderItem.Count;
                    order.OrderItems.Add(orderItem);
                }           
            }


            else
            {
                string basketlist = HttpContext.Request.Cookies["BasketItems"];
                if(basketlist!= null)
                {
					basketitems = JsonConvert.DeserializeObject<List<BasketItemsViewModel>>(basketlist);
                    foreach(var item in basketitems)
                    {
                        Book book = _context.Books.FirstOrDefault(x=>x.Id == item.BookId);
                        OrderItem orderItem = new OrderItem
                        {
                            Book = book,
                            BookName = book.Name,
                            Order = order,
                            CostPrice = book.CostPrice,
                            SalePrice = book.SalePrice * (1 - book.DiscountPrice / 100),
                            Count = item.Count,
                            DiscountPrice = book.DiscountPrice,
                        };
						totalprice += orderItem.SalePrice * orderItem.Count;
						order.OrderItems.Add(orderItem);
                    }
                   HttpContext.Response.Cookies.Delete("BasketItems");
				}
            }
            order.TotalPrice= totalprice;
            _context.Orders.Add(order);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        
    }
}