using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PustokImagesUploadTask.Models;
using PustokImagesUploadTask.ViewModels;
using System.Linq.Expressions;

namespace PustokImagesUploadTask.ViewComponents
{
    public class PriceViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public PriceViewComponent(AppDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            double price = 0;
            int count = 0;
            List<CheckOutViewModel> checkOutItems = new List<CheckOutViewModel>();
            List<BasketItemsViewModel> basketItems = new List<BasketItemsViewModel>();
            AppUser member = null;
            if (User.Identity.IsAuthenticated)
            {
                member = await _userManager.FindByNameAsync(User.Identity.Name);
            }
            if(member is not null)
            {
                List<BasketItem> memberbasketitems = _context.BasketItems.Include(x=>x.Book).Where(x => x.AppUserId == member.Id && x.IsDeleted == false).ToList();
                foreach(var item in memberbasketitems)
                {
                    CheckOutViewModel checkOutViewModel = new CheckOutViewModel()
                    {
                        Book = item.Book,
                        Count = item.Count
                    };
                    price += checkOutViewModel.Book.SalePrice * (1 - checkOutViewModel.Book.DiscountPrice / 100)*checkOutViewModel.Count;
                    count += checkOutViewModel.Count;
                }
            }
            else
            {

            string existitems = HttpContext.Request.Cookies["BasketItems"];

            if (existitems is not null)
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketItemsViewModel>>(existitems);
                foreach (var item in basketItems)
                {
                    CheckOutViewModel basketItem = new CheckOutViewModel()
                    {
                        Book = _context.Books.Include(x => x.BookImages).FirstOrDefault(x => x.Id == item.BookId),
                        Count = item.Count
                    };
                    price += (basketItem.Count * (basketItem.Book.SalePrice * (1 - basketItem.Book.DiscountPrice / 100)));
                    count += item.Count;
                }
            }

            }
            PriceViewModel priceViewModel = new PriceViewModel()
            {
                Price = price,
                Count= count
            };
            return View(priceViewModel);
        }
    }
}
