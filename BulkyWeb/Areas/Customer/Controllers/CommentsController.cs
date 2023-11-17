using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    public class CommentsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CommentsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //GET COMMENTS
        [HttpPost]
        public JsonResult LeaveComment(ReviewVM model)
        {
            Review review = new Review();
            review.Comment = model.Comment;
            review.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            review.ShoppingCartId = model.ShoppingCartId;
            review.ReviewId = model.ReviewId;
            review.Rating = 1;
            review.CommentedOn = DateTime.Now;
            JsonResult json = new JsonResult(review);
            var result = false;
            result = _unitOfWork.Review.AddNewReview(review);
            if (result)
            {
                return Json(new { Success = true });
            }
            else
            {
                return Json(new { Success = false, message = "Unable to perform operation" });
            }
           // return Json(new { Success = _unitOfWork.Review.AddNewReview(review) });

         
        }
    }
}
