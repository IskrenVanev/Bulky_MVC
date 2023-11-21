using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Models.ViewModels;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
       

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {

            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        }
        public IActionResult Details(int productId)//Fix the review problem
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages,Reviews"),
                Count = 1,
                ProductId = productId
               
            }; 

            // Update the count to be at least 1 if it's less than 1
            if (cart.Count < 1)
            {
                cart.Count = 1;
            }
        
            return View(cart);
        }

       



        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u =>
                u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);
            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u =>
                    u.ApplicationUserId == userId).Count());
            }

            TempData["success"] = "Cart updated successfully";
           
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}







//if (ModelState.IsValid)
//{
//    if (cart.Product.Review.Any(r => r.Rating == newReview.Rating && r.Comment == newReview.Comment))
//    {
//        // Review with the same rating and comment already exists, handle accordingly
//        TempData["warning"] = "Review with the same rating and comment already exists.";
//    }
//    foreach (var review in cart.Product.Review)
//    {
//        var Review = new Review
//        {
//            Rating = review.Rating,
//            Comment = review.Comment,
//            ProductId = cart.ProductId
//        };

//        _unitOfWork.Review.Add(Review);
//    }


//    _unitOfWork.Save();

//    TempData["success"] = "Review submitted successfully";
//}