using BulkyBook.Models;

namespace BulkyBookWeb.Services
{
    public interface ICompanyService
    {
        IEnumerable<Company> GetAllCompanies();
        Company? GetCompanyById(int id);
        void UpsertCompany(Company company);
        bool DeleteCompany(int id);
    }
}
