using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using BulkyBookWeb.Services;
using Microsoft.AspNetCore.Authorization;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        [BindProperty]  
        public OrderVM OrderVm { get; set; }

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public IActionResult Index()
        {
            return View("Index");
        }

        public IActionResult Details(int orderId)
        {
            OrderVM? orderVm = _orderService.GetOrderDetails(orderId);
            if (orderVm == null)
            {
                return NotFound();
            }

            OrderVm = orderVm;
            return View("Details",OrderVm);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin+ ","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            int? orderId = _orderService.UpdateOrderDetail(OrderVm.OrderHeader);
            if (!orderId.HasValue)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new {orderId = orderId.Value});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            bool isUpdated = _orderService.StartProcessing(OrderVm.OrderHeader.Id);
            if (!isUpdated)
            {
                return NotFound();
            }

            TempData["Success"] = "Order Details Updated Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            bool isShipped = _orderService.ShipOrder(OrderVm.OrderHeader);
            if (!isShipped)
            {
                return NotFound();
            }

            TempData["Success"] = "Order Shipped Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            bool isCancelled = _orderService.CancelOrder(OrderVm.OrderHeader.Id);
            if (!isCancelled)
            {
                return NotFound();
            }

            TempData["Success"] = "Order cancelled Successfully.";
            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            string? sessionUrl = _orderService.CreateStripeSession(OrderVm.OrderHeader.Id, domain);
            if (string.IsNullOrWhiteSpace(sessionUrl))
            {
                return NotFound();
            }

            Response.Headers.Add("Location", sessionUrl);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            _orderService.ConfirmPayment(orderHeaderId);

            return View(orderHeaderId);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            string? userId = null;
            bool isPrivilegedUser = User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);
            if (!isPrivilegedUser)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }

            IEnumerable<OrderHeader> objOrderHeaders = _orderService.GetOrders(status, isPrivilegedUser, userId);
            return Json(new { data = objOrderHeaders });
        }
        #endregion
    }
}