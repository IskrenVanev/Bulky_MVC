using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.Services;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = _cartService.GetShoppingCart(userId);

            return View(ShoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = _cartService.GetSummary(userId);

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            int orderHeaderId = _cartService.PrepareOrder(ShoppingCartVM, userId);

            ApplicationUser applicationUser = ShoppingCartVM.OrderHeader.ApplicationUser;
            // Note: In a real app, you might want to re-fetch or ensure applicationUser is populated correctly in the service
            // but for now, following the existing logic.

            if (ShoppingCartVM.OrderHeader.PaymentStatus == SD.PaymentStatusPending)
            {
                //stripe logic
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                string sessionUrl = _cartService.CreateStripeSession(orderHeaderId, userId, domain);

                Response.Headers.Add("Location", sessionUrl);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = orderHeaderId });
        }

        public IActionResult OrderConfirmation(int id)
        {
            _cartService.ProcessOrderConfirmation(id);
            HttpContext.Session.SetInt32(SD.SessionCart, 0);
            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            _cartService.Plus(cartId);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            int count = _cartService.Minus(cartId);
            if (count == 0)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                
                HttpContext.Session.SetInt32(SD.SessionCart, _cartService.GetCartCount(userId));
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            _cartService.Remove(cartId);
            
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            HttpContext.Session.SetInt32(SD.SessionCart, _cartService.GetCartCount(userId));
            
            return RedirectToAction(nameof(Index));
        }
    }
}