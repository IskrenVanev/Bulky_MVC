using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using BulkyBook.Models.ViewModels;
using BulkyBookWeb.Services;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;
        private readonly ICartService _cartService;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeService, ICartService cartService, ICategoryService categoryService)
        {
            _logger = logger;
            _homeService = homeService;
            _cartService = cartService;
            _categoryService = categoryService;
        }

        public IActionResult Index(string status)
        {
            IEnumerable<Product> productList = _homeService.GetAllProducts(status);
            ViewBag.CategoryList = _categoryService.GetAllCategories();
            return View(productList);
        }

        [HttpGet]
        public IActionResult Details(int productId)
        {
            ShoppingCart cart = _homeService.GetProductDetails(productId);
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            _homeService.AddToCart(shoppingCart, userId);

            HttpContext.Session.SetInt32(SD.SessionCart, _cartService.GetCartCount(userId));

            TempData["success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment(string content, int ProductId, int rating)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var userNameClaim = claimsIdentity.FindFirst(ClaimTypes.Name);
            var userName = userNameClaim != null ? userNameClaim.Value : string.Empty;

            await _homeService.AddCommentAsync(content, ProductId, userId, userName, rating);

            return RedirectToAction("Details", "Home", new { productId = ProductId });
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