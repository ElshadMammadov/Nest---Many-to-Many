using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PustokImagesUploadTask.Models;
using PustokImagesUploadTask.ViewModels;

namespace PustokImagesUploadTask.ViewComponents
{
    public class BasketViewComponent : ViewComponent
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public BasketViewComponent(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<CheckOutViewModel> checkOutItems = new List<CheckOutViewModel>();
            List<BasketItemsViewModel> basketItems = new List<BasketItemsViewModel>();
            AppUser member = null;
            if (User.Identity.IsAuthenticated)
            {
                member = await _userManager.FindByNameAsync(User.Identity.Name);
            }
            if (member is not null)
            {
                List<BasketItem> memberbasketitems = _context.BasketItems.Include(x => x.Book).Where(x => x.AppUserId == member.Id && x.IsDeleted==false).ToList();
                foreach (var item in memberbasketitems)
                {
                    CheckOutViewModel checkOut = new CheckOutViewModel()
                    {
                        Book = item.Book,
                        Count = item.Count
                    };
                    checkOutItems.Add(checkOut);
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
                        checkOutItems.Add(basketItem);
                    }
                }
            }




            return View(checkOutItems);
        }
    }
}
