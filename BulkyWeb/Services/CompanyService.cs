using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;

namespace BulkyBookWeb.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Company> GetAllCompanies()
        {
            return _unitOfWork.Company.GetAll();
        }

        public Company? GetCompanyById(int id)
        {
            return _unitOfWork.Company.Get(u => u.Id == id);
        }

        public void UpsertCompany(Company company)
        {
            if (company.Id == 0)
            {
                _unitOfWork.Company.Add(company);
            }
            else
            {
                _unitOfWork.Company.Update(company);
            }

            _unitOfWork.Save();
        }

        public bool DeleteCompany(int id)
        {
            Company? companyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
            if (companyToBeDeleted == null)
            {
                return false;
            }

            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();
            return true;
        }
    }
}
