using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        
        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
            
        }

        public IActionResult Index()
        {
            List<Company> objCompanyList = _companyService.GetAllCompanies().ToList();

            return View("Index",objCompanyList);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company? companyObj = _companyService.GetCompanyById(id.Value);
                if (companyObj == null)
                {
                    return NotFound();
                }
                return View(companyObj);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company CompanyObj)
        {
            if (ModelState.IsValid)
            {
                _companyService.UpsertCompany(CompanyObj);
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {
               
                return View("Upsert",CompanyObj);
            }
        }

        
        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _companyService.GetAllCompanies().ToList();
            return Json(new { data = objCompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            bool isDeleted = _companyService.DeleteCompany(id.Value);
            if (!isDeleted)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            return Json(new { success = true, message = "Delete successful" });
        }
        #endregion
    }
}