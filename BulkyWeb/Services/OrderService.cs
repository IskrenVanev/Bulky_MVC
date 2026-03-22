using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Stripe;
using Stripe.Checkout;

namespace BulkyBookWeb.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public OrderVM? GetOrderDetails(int orderId)
        {
            OrderHeader? orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser");
            if (orderHeader == null)
            {
                return null;
            }

            return new OrderVM
            {
                OrderHeader = orderHeader,
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
        }

        public int? UpdateOrderDetail(OrderHeader inputOrderHeader)
        {
            OrderHeader? orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == inputOrderHeader.Id);
            if (orderHeaderFromDb == null)
            {
                return null;
            }

            orderHeaderFromDb.Name = inputOrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = inputOrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = inputOrderHeader.StreetAddress;
            orderHeaderFromDb.City = inputOrderHeader.City;
            orderHeaderFromDb.State = inputOrderHeader.State;
            orderHeaderFromDb.PostalCode = inputOrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(inputOrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = inputOrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(inputOrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = inputOrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            return orderHeaderFromDb.Id;
        }

        public bool StartProcessing(int orderId)
        {
            OrderHeader? orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
            if (orderHeader == null)
            {
                return false;
            }

            _unitOfWork.OrderHeader.UpdateStatus(orderId, SD.StatusInProcess);
            _unitOfWork.Save();
            return true;
        }

        public bool ShipOrder(OrderHeader inputOrderHeader)
        {
            OrderHeader? orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == inputOrderHeader.Id);
            if (orderHeader == null)
            {
                return false;
            }

            orderHeader.TrackingNumber = inputOrderHeader.TrackingNumber;
            orderHeader.Carrier = inputOrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            return true;
        }

        public bool CancelOrder(int orderId)
        {
            OrderHeader? orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId);
            if (orderHeader == null)
            {
                return false;
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();
            return true;
        }

        public string? CreateStripeSession(int orderId, string domain)
        {
            OrderHeader? orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser");
            if (orderHeader == null)
            {
                return null;
            }

            IEnumerable<OrderDetail> orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product");

            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            return session.Url;
        }

        public void ConfirmPayment(int orderHeaderId)
        {
            OrderHeader? orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderHeaderId);
            if (orderHeader == null)
            {
                return;
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
        }

        public IEnumerable<OrderHeader> GetOrders(string? status, bool isPrivilegedUser, string? userId)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (isPrivilegedUser)
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                orderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }

            return orderHeaders;
        }
    }
}
